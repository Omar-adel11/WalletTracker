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

             

             builder.ComplexProperty(i => i.Amount, a =>
             {
                 a.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                 a.Property(m => m.Currency).HasMaxLength(3);
                 a.Property(m => m.Currency).HasDefaultValue("EGP");
             });
        }
    }
}
