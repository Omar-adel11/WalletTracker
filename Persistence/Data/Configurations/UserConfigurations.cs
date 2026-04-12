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
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ComplexProperty(u => u.Balance, b =>
                {
                    b.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                    b.Property(m => m.Currency).HasMaxLength(3);
                    b.Property(m => m.Currency).HasDefaultValue("EGP");
                });
        }
    }
}
