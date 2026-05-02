using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.SubsriptionDTO;

namespace ServiceAbstraction
{
    public interface ISubscriptionService
    {
        Task<PaymentInitiatedDTO> InitiatePaymentAsync(int userId, InitiatePaymentDTO dto);

        Task HandleWebhookAsync(string rawBody, string hmacHeader);

        Task<SubscriptionStatusDTO> GetSubscriptionStatusAsync(int userId);
    }
}
