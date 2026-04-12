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
    public class BudgetConfigurations : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.HasOne(b=> b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasQueryFilter(b => !b.IsDisabled);

            builder.ComplexProperty(b => b.Limit, a =>
             {
                 a.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                 a.Property(m => m.Currency).HasMaxLength(3);
                 a.Property(m => m.Currency).HasDefaultValue("EGP");
             });

            builder.ComplexProperty(b => b.Spent, a =>
            {
                a.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                a.Property(m => m.Currency).HasMaxLength(3);
                a.Property(m => m.Currency).HasDefaultValue("EGP");
            });
        }
    }
}
