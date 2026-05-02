using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace ServiceAbstraction.DTOs.SubsriptionDTO
{
    public class InitiatePaymentDTO
    {
        public SubscriptionPlan Plan { get; set; }
    }
}
