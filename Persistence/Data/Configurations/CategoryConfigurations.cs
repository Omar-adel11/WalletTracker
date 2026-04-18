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
    public class CategoryConfigurations : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasData(SeedCategories());
        }

        private static List<Category> SeedCategories()
        {
            return new List<Category>
        {
            new Category
            {
                id = 1, // Manual ID is required for HasData
                Name = "Food & Drinks",
                CreatedAt = new DateTimeOffset(2026, 4, 13, 9, 1, 2, TimeSpan.Zero)
            },
            new Category
            {
                id = 2,
                Name = "Transportation",
                CreatedAt = new DateTimeOffset(2026, 4, 13, 9, 1, 2, TimeSpan.Zero)
            },
            new Category
            {
                id = 3,
                Name = "Shopping",
                CreatedAt = new DateTimeOffset(2026, 4, 13, 9, 1, 2, TimeSpan.Zero)
            },
            new Category
            {
                id = 4,
                Name = "Housing & Utilities",
                CreatedAt = new DateTimeOffset(2026, 4, 13, 9, 1, 2, TimeSpan.Zero)
            },
            new Category
            {
                id = 5,
                Name = "Entertainment",
                CreatedAt = new DateTimeOffset(2026, 4, 13, 9, 1, 2, TimeSpan.Zero)
            }
        };
        }
    }
}
