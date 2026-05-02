using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.SubsriptionDTO
{
    public class PaymentInitiatedDTO
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string IntentionId { get; set; } = string.Empty;
    }
}
