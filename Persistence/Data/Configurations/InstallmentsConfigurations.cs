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
    public class InstallmentsConfigurations : IEntityTypeConfiguration<Installments>
    {
        public void Configure(EntityTypeBuilder<Installments> builder)
        {
            builder.HasOne(i => i.User)
                   .WithMany(u => u.Installments)
                   .HasForeignKey(i => i.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

             builder.HasOne(i => i.Category)
                   .WithMany(c => c.Installments)
                   .HasForeignKey(i => i.CategoryId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.Property(i => i.NoOfPaidInstallments).HasDefaultValue(0);


            builder.ComplexProperty(t => t.Amount, m => m.ConfigureMoney("Amount"));
        }
    }
}
