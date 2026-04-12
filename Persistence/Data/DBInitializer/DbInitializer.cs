using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Persistence.Data.Contexts;

namespace Persistence.Data.DBInitializer
{
    public class DbInitializer(AppDbContext _appDbContext,
        UserManager<User> _userManager,
        RoleManager<IdentityRole<int>> _roleManager,
        IConfiguration _configuration) : IDbInitializer
    {
        public async Task InitializeAsync()
        {
            var Database = _appDbContext.Database;
            try
            {
                if((await Database.GetPendingMigrationsAsync()).Any())
                {
                    await Database.MigrateAsync();
                }

                await SeedRoleAsync();
                await SeedUserAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error during initialization: {ex.Message}");
                throw;
            }
        }

        private async Task SeedRoleAsync()
        {
            if(!await _roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new IdentityRole<int>("Admin");
                await _roleManager.CreateAsync(adminRole);
            }
        }

        private async Task SeedUserAsync()
        {
            var adminEmail = _configuration["AdminCredentials:DefaultAdmin:Email"];
            var adminPassword = _configuration["AdminCredentials:DefaultAdmin:Password"];

            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = _configuration["AdminCredentials:DefaultAdmin:FirstName"],
                    LastName = _configuration["AdminCredentials:DefaultAdmin:LastName"],
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                var result = await _userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
