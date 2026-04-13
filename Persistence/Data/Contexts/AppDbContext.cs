using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interceptors;

namespace Persistence.Data.Contexts
{
    public class AppDbContext : IdentityDbContext<User,IdentityRole<int>,int>
    {
        private readonly SoftDeleteInterceptor _softDeleteInterceptor;
        public AppDbContext(DbContextOptions<AppDbContext> options,
                            SoftDeleteInterceptor softDeleteInterceptor) : base(options)
        {
            _softDeleteInterceptor = softDeleteInterceptor;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.AddInterceptors(_softDeleteInterceptor);

        }
    }
}
