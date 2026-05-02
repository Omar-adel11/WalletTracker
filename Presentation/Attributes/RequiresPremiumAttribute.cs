using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ServiceAbstraction;
using Shared.Errors;

namespace Presentation.Attributes
{
    public class RequiresPremiumAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var userIdClaim = context.HttpContext.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userId = int.Parse(userIdClaim);
            var serviceManager = context.HttpContext.RequestServices
                .GetRequiredService<IServiceManager>();

            var status = await serviceManager.SubscriptionService
                .GetSubscriptionStatusAsync(userId);

            if (!status.IsPremium)
            {
                context.Result = new ObjectResult(new ErrorDetails
                {
                    StatusCode = 403,
                    Message = "This feature requires a Premium subscription."
                })
                { StatusCode = 403 };
                return;
            }

            await next();
        }
    }
}
