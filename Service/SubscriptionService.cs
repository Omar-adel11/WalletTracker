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
    public class SubscriptionService(IUnitOfWork _unitOfWork, IPaymentProvider paymentProvider,PaymobClient _paymobClient,IOptions<PaymobSettings> _paymobOptions) : ISubscriptionService
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

        
        public async Task HandleWebhookAsync(string rawBody, string hmacHeader)
        {
            // 1. Verify HMAC signature to ensure the request is from Paymob
            await paymentProvider.VerifySignature(rawBody, hmacHeader);

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
    .GetProperty("extra"); 

            if (!extras.TryGetProperty("subscription_id", out var subIdEl))
                return; // Not our webhook, ignore

            var subscriptionId = int.Parse(subIdEl.GetString()!);

            // 3. Find the subscription
            var subscription = await _unitOfWork.Repository<Subscription>()
                .GetByIdAsync(subscriptionId,s=>s.User);

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

                subscription.User.IsPremium = true;
                    
                
            }
            else
            {
                subscription.Status = SubscriptionStatus.Cancelled;
            }

            subscription.UpdatedAt = DateTimeOffset.UtcNow;
            _unitOfWork.Repository<Subscription>().Update(subscription);
            await _unitOfWork.CompleteAsync();
        }

        
    }
}
