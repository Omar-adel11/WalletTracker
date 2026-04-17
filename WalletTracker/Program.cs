
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using Persistence.Data.Contexts;
using Persistence.Data.DBInitializer;
using Persistence.Interceptors;
using Persistence.Repository;
using Service.Mapping.Budget;
using Service.Mapping.Wallet;

namespace WalletTracker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region Register services

            var ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddSingleton<SoftDeleteInterceptor>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(ConnectionString);
            });

            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                // Lockout settings (Prevents Brute Force)
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();


            builder.Services.AddAutoMapper(typeof(BudgetProfile).Assembly);

            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            #endregion


            var app = builder.Build();

            #region Self-Healing DB
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var services = scope.ServiceProvider;
                    var dbInitializer = services.GetRequiredService<IDbInitializer>();
                    await dbInitializer.InitializeAsync();
                    Console.WriteLine("Database initialized and seeded successfully!");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during migration: {ex.Message}");
                }
            }
            
            #endregion

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
