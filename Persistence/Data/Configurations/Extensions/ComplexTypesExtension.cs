using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public static class ComplexTypesExtension
    {
        public static void ConfigureMoney(this ComplexPropertyBuilder<Money> builder, string prefix)
            
        {
            builder.Property("Amount")
                   .HasColumnName($"{prefix}_Amount")
                   .HasColumnType("decimal(18,2)");

            builder.Property("Currency")
                   .HasColumnName($"{prefix}_Currency")
                   .HasMaxLength(3)
                   .HasDefaultValue("EGP");
        }
    }
}
