using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Data.Configurations.Extensions;

namespace Persistence.Data.Configurations
{
    public class TransactionConfigurations : IEntityTypeConfiguration<Domain.Entities.Transaction>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Transaction> builder)
        {
            builder.HasOne(t => t.User)
                   .WithMany(u => u.Transactions)
                   .HasForeignKey(t => t.UserId);

            builder.HasOne(t => t.Wallet)
                   .WithMany(w => w.Transactions)
                    .HasForeignKey(t => t.WalletId)
                    .OnDelete(DeleteBehavior.NoAction);

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


            builder.ComplexProperty(t => t.Amount, m => m.ConfigureMoney("Amount"));
        }
    }
}
