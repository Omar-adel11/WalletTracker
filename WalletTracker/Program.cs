
using System.Text;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Writers;
using Persistence.Data.Contexts;
using Persistence.Data.DBInitializer;
using Persistence.Interceptors;
using Persistence.Repository;
using Service;
using Service.Helper;
using Service.Mapping.Budget;
using Service.Mapping.Wallet;
using ServiceAbstraction;
using ServiceAbstraction.Helper.Email;
using Shared.Errors;
using WalletTracker.Middlewares;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            #region Auth
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });
            #endregion

            #region Email
            builder.Services.Configure<EmailSettings>(
                    builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();
            #endregion

            #region Application Services
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<IBudgetService, BudgetService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IItemToBuyService, ItemToBuyService>();
            builder.Services.AddScoped<IInstallmentsService, InstallmentsService>();
            builder.Services.AddScoped<IServiceManager, ServiceManager>();
            #endregion

            #region ValidationError
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {

                options.InvalidModelStateResponseFactory = ActionContext =>
                {
                    var errors = ActionContext.ModelState.Where(m => m.Value.Errors.Any())
                                                          .Select(m => new ValidationError()
                                                          {
                                                              Field = m.Key,
                                                              Error = m.Value.Errors.Select(e => e.ErrorMessage)
                                                          });
                    var response = new ValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(response);
                };
               
            });
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
            app.UseStaticFiles();
            app.UseMiddleware<GlobalErrorHandlingMiddleware>();
            app.UseHttpsRedirection();  

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
