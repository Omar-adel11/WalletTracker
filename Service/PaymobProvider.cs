using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PaymobProvider(IUnitOfWork _unitOfWork,IOptions<PaymobSettings> _paymobOptions,PaymobClient _paymobClient) : IPaymentProvider
    {
        public async Task<PaymentInitiatedDTO> InitiatePaymentAsync(int userId, InitiatePaymentDTO dto)
        {

            var users = await _unitOfWork.Repository<User>()
                .GetAsync(u => u.Id == userId);
            var user = users.FirstOrDefault()
                ?? throw new UserNotFoundNullException();


            var amountCents = dto.Plan == SubscriptionPlan.Premium
                ? _paymobOptions.Value.PremiumAmountCents
                : 0;

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
                payment_methods: new List<int> { int.Parse(_paymobOptions.Value.IntegrationId) },
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


        public Task VerifySignature(string rawBody, string hmacHeader)
        {
            using var doc = JsonDocument.Parse(rawBody);
            var obj = doc.RootElement.GetProperty("obj");

            var data = BuildHmacString(obj);

            var keyBytes = Encoding.UTF8.GetBytes(_paymobOptions.Value.WebhookHmacSecret);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            Console.WriteLine("HMAC STRING: " + data);
            using var hmac = new HMACSHA512(keyBytes);
            var computed = BitConverter.ToString(hmac.ComputeHash(dataBytes))
                .Replace("-", "")
                .ToLower();

            if (computed != hmacHeader.ToLower())
                throw new UnauthorizedAccessException("Invalid Paymob webhook signature.");
            return Task.CompletedTask;
        }

       
        private string BuildHmacString(JsonElement obj)
        {
            return string.Concat(
                obj.GetProperty("amount_cents").ToString(),
                obj.GetProperty("created_at").ToString(),
                obj.GetProperty("currency").ToString(),
                obj.GetProperty("error_occured").ToString().ToLower(),
                obj.GetProperty("has_parent_transaction").ToString().ToLower(),
                obj.GetProperty("id").ToString(),
                obj.GetProperty("integration_id").ToString(),
                obj.GetProperty("is_3d_secure").ToString().ToLower(),
                obj.GetProperty("is_auth").ToString().ToLower(),
                obj.GetProperty("is_capture").ToString().ToLower(),
                obj.GetProperty("is_refunded").ToString().ToLower(),
                obj.GetProperty("is_standalone_payment").ToString().ToLower(),
                obj.GetProperty("is_voided").ToString().ToLower(),
                obj.GetProperty("order").GetProperty("id").ToString(),
                obj.GetProperty("owner").ToString(),
                obj.GetProperty("pending").ToString().ToLower(),
                obj.GetProperty("source_data").GetProperty("pan").ToString(),
                obj.GetProperty("source_data").GetProperty("sub_type").ToString(),
                obj.GetProperty("source_data").GetProperty("type").ToString(),
                obj.GetProperty("success").ToString().ToLower()
            );
        }
    }
}
