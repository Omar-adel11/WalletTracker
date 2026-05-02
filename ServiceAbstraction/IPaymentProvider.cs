using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.SubsriptionDTO;

namespace ServiceAbstraction
{
    public interface IPaymentProvider
    {
        Task<PaymentInitiatedDTO> InitiatePaymentAsync(int userId, InitiatePaymentDTO dto);
        Task VerifySignature(string rawBody, string hmacHeader);
    }
}
