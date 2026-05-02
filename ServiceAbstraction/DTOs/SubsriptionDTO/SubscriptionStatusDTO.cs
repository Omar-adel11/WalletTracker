using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace ServiceAbstraction.DTOs.SubsriptionDTO
{
    public class SubscriptionStatusDTO
    {
        public bool IsPremium { get; set; }
        public SubscriptionPlan Plan { get; set; }
        public SubscriptionStatus Status { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
