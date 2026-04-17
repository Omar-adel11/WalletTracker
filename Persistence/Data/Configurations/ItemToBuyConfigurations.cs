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
    public class ItemToBuyConfigurations : IEntityTypeConfiguration<ItemToBuy>
    {
        public void Configure(EntityTypeBuilder<ItemToBuy> builder)
        {
            builder.HasOne(i => i.User)
                   .WithMany(u => u.ItemsToBuy)
                   .HasForeignKey(i => i.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(i => i.Wallet)
                   .WithMany(w => w.ItemsToBuy)
                   .HasForeignKey(i => i.WalletId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(i => i.Category)
                   .WithMany(c => c.ItemsToBuy)
                   .HasForeignKey(i => i.CategoryId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.ComplexProperty(t => t.Amount, m => m.ConfigureMoney("Amount"));

            builder.ComplexProperty(t => t.Price, m => m.ConfigureMoney("Price"));
        }
    }
}
