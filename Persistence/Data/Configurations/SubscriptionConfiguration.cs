using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Domain.Entities.Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.HasOne( s=> s.User)
                   .WithMany(User => User.Subscriptions)
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(s => s.PaymobOrderId).HasMaxLength(100);
            builder.Property(s => s.PaymobIntentionId).HasMaxLength(200);
            builder.Property(s => s.TransactionId).HasMaxLength(100);

            builder.HasIndex(s => new { s.UserId, s.Status });
        }
    }
}
