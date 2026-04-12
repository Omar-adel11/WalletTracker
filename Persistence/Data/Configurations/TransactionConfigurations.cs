using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class TransactionConfigurations : IEntityTypeConfiguration<Domain.Entities.Transaction>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Transaction> builder)
        {
            builder.HasOne(t => t.User)
                   .WithMany(u => u.Transactions)
                   .HasForeignKey(t => t.UserId);

            builder.HasOne(c => c.Category)
                   .WithMany(u => u.Transactions)
                   .HasForeignKey(c => c.CategoryId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(t => t.Installment)
           .WithMany(i => i.transactions)
           .HasForeignKey(t => t.InstallmentsId)
           .OnDelete(DeleteBehavior.NoAction); //    keep the payment even if the installment plan is deleted

            builder.HasQueryFilter(t => !t.IsDeleted);
            
            builder.HasIndex(t => t.IsDeleted)
                   .HasFilter("[IsDeleted] = 0");


            builder.ComplexProperty(t => t.Amount, a =>
            {
               a.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                a.Property(m => m.Currency).HasMaxLength(3);
                a.Property(m => m.Currency).HasDefaultValue("EGP");
            });
        }
    }
}
