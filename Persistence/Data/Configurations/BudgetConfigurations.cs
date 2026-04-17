using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Data.Configurations.Extensions;

namespace Persistence.Data.Configurations
{
    public class BudgetConfigurations : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.HasOne(b=> b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(b=> b.Wallet)
                .WithMany(w => w.Budgets)
                .HasForeignKey(b => b.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasQueryFilter(b => !b.IsDisabled);

            builder.ComplexProperty(t => t.Limit, m => m.ConfigureMoney("Limit"));
            builder.ComplexProperty(t => t.Spent, m => m.ConfigureMoney("Spent"));

        }
    }
}
