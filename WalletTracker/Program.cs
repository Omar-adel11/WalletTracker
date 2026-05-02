
using System.Text;
using System.Threading.RateLimiting;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistence.Data.Contexts;
using Persistence.Data.DBInitializer;
using Persistence.Interceptors;
using Persistence.Repository;
using Service;
using Service.Helper;
using Service.Helper.Cache;
using Service.Mapping.Budget;
using ServiceAbstraction;
using ServiceAbstraction.Helper.Email;
using Shared.Errors;
using StackExchange.Redis;
using WalletTracker.Middlewares;
using static Service.Helper.Paymob;

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
            builder.Services.AddSingleton<ICacheService, CacheService>();
            builder.Services.AddSingleton<ICacheRepository, CacheRepository>();
            builder.Services.AddSingleton<IConnectionMultiplexer>((ServiceProvider) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost";
                var config = ConfigurationOptions.Parse(connectionString);
                config.AbortOnConnectFail = false; // Don't crash if Redis is down
                config.ConnectRetry = 3;
                config.ReconnectRetryPolicy = new ExponentialRetry(5000);
                return ConnectionMultiplexer.Connect(config);
            });
            builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("CacheSettings"));
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
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
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

            #region RateLimiter
            builder.Services.AddRateLimiter(LimiterOptions =>
            {
                LimiterOptions.AddPolicy("AuthPolicy", context =>
                                         RateLimitPartition.GetFixedWindowLimiter(
                                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                                        _ => new FixedWindowRateLimiterOptions
                                        {
                                            PermitLimit = 5,
                                            Window = TimeSpan.FromMinutes(1),
                                            QueueLimit = 0
                                        }));

                LimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                                        RateLimitPartition.GetSlidingWindowLimiter(
                                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                                        _ => new SlidingWindowRateLimiterOptions
                                        {
                                            PermitLimit = 100,
                                            Window = TimeSpan.FromMinutes(1),
                                            SegmentsPerWindow = 4,
                                            QueueLimit = 10,
                                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                                        }));


                LimiterOptions.AddPolicy("AnalyticsPolicy", context =>
                                RateLimitPartition.GetFixedWindowLimiter(
                                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                                    _ => new FixedWindowRateLimiterOptions
                                    {

                                        PermitLimit = 20,
                                        Window = TimeSpan.FromMinutes(1),
                                        QueueLimit = 0
                                    })
                                );

                LimiterOptions.OnRejected = async (context, CancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers["Retry-After"] = "60";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        StatusCode = 429,
                        Message = "Too many requests. Please try again later."
                    }, CancellationToken);
                };
             
            });
            #endregion

            #region paymob
            builder.Services.Configure<PaymobSettings>(
    builder.Configuration.GetSection("PaymobSettings"));

            builder.Services.AddHttpClient<PaymobClient>();

            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
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
            app.UseRateLimiter();
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
