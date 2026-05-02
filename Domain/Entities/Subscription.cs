using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public SubscriptionPlan Plan { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;

        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        // Paymob tracking fields
        public string? PaymobOrderId { get; set; }
        public string? PaymobIntentionId { get; set; }
        public string? TransactionId { get; set; }

        public bool IsActive => Status == SubscriptionStatus.Active
                             && EndDate > DateTimeOffset.UtcNow;
    }
}
