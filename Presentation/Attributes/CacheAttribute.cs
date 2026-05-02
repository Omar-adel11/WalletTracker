using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using Service.Helper.Cache;
using ServiceAbstraction;

namespace Presentation.Attributes
{
    public class CacheAttribute(string Key) : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IServiceManager>().CacheService;

            var settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<CacheSettings>>().Value;

            
            TimeSpan ttl = Key switch
            {
                "Category" => TimeSpan.FromHours(settings.CategoryExpirationHours),
                "Dashboard" => TimeSpan.FromMinutes(settings.DashboardExpirationMinutes),
                "Wallet" => TimeSpan.FromMinutes(settings.WalletExpirationMinutes),
                _ => TimeSpan.FromMinutes(30) 
            };

            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
            var response = await cache.GetCacheValueAsync(cacheKey);
            if(response is not null)
            {
                context.Result = new ContentResult()
                {
                    ContentType = "application/json",
                    StatusCode = 200,
                    Content = response
                };
                return;
            }
            var executedContext = await next.Invoke();
            if(executedContext.Result is ObjectResult objectResult)
            {
                var resultValue = objectResult.Value;
                if(resultValue is not null)
                {
                    await cache.SetCacheValueAsync(cacheKey, resultValue, ttl);
                }
            }

        }
        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var item in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{item.Key}-{item.Value}");
            }
            return keyBuilder.ToString();

        }
    }
}
