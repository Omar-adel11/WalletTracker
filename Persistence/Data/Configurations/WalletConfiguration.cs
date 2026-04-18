using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Struct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.Property(w => w.Credit).HasColumnType("decimal(18,2)");
            builder.Property(w => w.Cash).HasColumnType("decimal(18,2)");
            builder.Property(w => w.Pended).HasColumnType("decimal(18,2)");

            builder.Ignore(w => w.TotalBalance);

            builder.HasIndex(W => new { W.UserId, W.Currency }).IsUnique();
        }
    }
}
