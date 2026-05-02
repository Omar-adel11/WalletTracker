using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.AuthExceptions;
using Microsoft.Extensions.Options;
using Service.Helper;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.SubsriptionDTO;
using static Service.Helper.Paymob;

namespace Service
{
    public class SubscriptionService(IUnitOfWork _unitOfWork,PaymobClient _paymobClient,IOptions<PaymobSettings> _paymobOptions) : ISubscriptionService
    {
        public async Task<SubscriptionStatusDTO> GetSubscriptionStatusAsync(int userId)
        {
            var subscriptions = await _unitOfWork.Repository<Subscription>()
                .GetAsync(s => s.UserId == userId
                            && s.Status == SubscriptionStatus.Active);

            var active = subscriptions
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();

            if (active == null || !active.IsActive)
                return new SubscriptionStatusDTO
                {
                    IsPremium = false,
                    Plan = SubscriptionPlan.Free,
                    Status = SubscriptionStatus.Expired
                };

            return new SubscriptionStatusDTO
            {
                IsPremium = true,
                Plan = active.Plan,
                Status = active.Status,
                EndDate = active.EndDate
            };
        }

        public async Task<PaymentInitiatedDTO> InitiatePaymentAsync(int userId, InitiatePaymentDTO dto)
        {
           
            var users = await _unitOfWork.Repository<User>()
                .GetAsync(u => u.Id == userId);
            var user = users.FirstOrDefault()
                ?? throw new UserNotFoundNullException();

            
            var amountCents = dto.Plan == SubscriptionPlan.Premium
                ? _paymobOptions.Value.PremiumAmountCents
                :0;

            var planName = dto.Plan == SubscriptionPlan.Premium
                ? "WalletTracker Premium"
                : "WalletTracker Free";

            // 3. Create a pending subscription record first so we can track it
            var subscription = new Subscription
            {
                UserId = userId,
                Plan = dto.Plan,
                Status = SubscriptionStatus.Pending,
                StartDate = DateTimeOffset.UtcNow,
                // EndDate is set after payment succeeds
                EndDate = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _unitOfWork.Repository<Subscription>().AddAsync(subscription);
            await _unitOfWork.CompleteAsync();

            // 4. Call Paymob Intention API
            var intention = new IntentionRequest(
                amount: amountCents,
                currency: _paymobOptions.Value.Currency,
                payment_methods: new List<string> { "Online Card" },
                items: new List<PaymobItem>
                {
                    new PaymobItem(planName, amountCents,"Premium subscription", 1)
                },
                billing_data: new BillingData(
                    first_name: user.FirstName,
                    last_name: user.LastName,
                    email: user.Email!,
                    phone_number: user.PhoneNumber ?? "N/A"
                ),
                notification_url: _paymobOptions.Value.NotificationUrl,
                redirection_url: _paymobOptions.Value.RedirectionUrl,
                // Pass our subscription ID in extras so the webhook can find it
                extras: new Dictionary<string, string>
                {
                    { "subscription_id", subscription.id.ToString() },
                    { "user_id", userId.ToString() }
                }
            );

            var result = await _paymobClient.CreateIntentionAsync(intention);

            // 5. Save the Paymob intention ID
            subscription.PaymobIntentionId = result.id;
            _unitOfWork.Repository<Subscription>().Update(subscription);
            await _unitOfWork.CompleteAsync();

            return new PaymentInitiatedDTO
            {
                ClientSecret = result.client_secret,
                PublicKey = _paymobOptions.Value.PublicKey,
                IntentionId = result.id
            };
        }

        public async Task HandleWebhookAsync(string rawBody, string hmacHeader)
        {
            // 1. Verify HMAC signature to ensure the request is from Paymob
            VerifyHmacSignature(rawBody, hmacHeader);

            // 2. Parse the webhook payload
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            // Paymob sends "obj" containing transaction details
            var obj = root.GetProperty("obj");
            var success = obj.GetProperty("success").GetBoolean();
            var transactionId = obj.GetProperty("id").GetInt64().ToString();

            // Extract our subscription_id from the extras/special_reference
            var extras = obj
                .GetProperty("payment_key_claims")
                .GetProperty("extras");

            if (!extras.TryGetProperty("subscription_id", out var subIdEl))
                return; // Not our webhook, ignore

            var subscriptionId = int.Parse(subIdEl.GetString()!);

            // 3. Find the subscription
            var subscription = await _unitOfWork.Repository<Subscription>()
                .GetByIdAsync(subscriptionId);

            if (subscription is null) return;

            // 4. Update based on payment result
            if (success)
            {
                subscription.Status = SubscriptionStatus.Active;
                subscription.TransactionId = transactionId;
                subscription.StartDate = DateTimeOffset.UtcNow;
                subscription.EndDate = subscription.Plan == SubscriptionPlan.Premium
                    ? DateTimeOffset.UtcNow.AddMonths(6)
                    : DateTimeOffset.UtcNow;
            }
            else
            {
                subscription.Status = SubscriptionStatus.Cancelled;
            }

            subscription.UpdatedAt = DateTimeOffset.UtcNow;
            _unitOfWork.Repository<Subscription>().Update(subscription);
            await _unitOfWork.CompleteAsync();
        }

        private void VerifyHmacSignature(string rawBody, string hmacHeader)
        {
            
            var keyBytes = Encoding.UTF8.GetBytes(_paymobOptions.Value.WebhookHmacSecret);
            var bodyBytes = Encoding.UTF8.GetBytes(rawBody);

            using var hmac = new HMACSHA512(keyBytes);
            var computed = Convert.ToHexString(hmac.ComputeHash(bodyBytes))
                                  .ToLower();

            if (computed != hmacHeader.ToLower())
                throw new UnauthorizedAccessException("Invalid Paymob webhook signature.");
        }

    }
}
