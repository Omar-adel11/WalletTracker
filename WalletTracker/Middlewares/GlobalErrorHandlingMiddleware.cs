using Domain.Exceptions.AuthExceptions;
using Domain.Exceptions.BadRequestException;
using Domain.Exceptions.MoneyInvalidOperationException;
using Domain.Exceptions.NullReferenceException;
using Shared;
using Shared.Errors;

namespace WalletTracker.Middlewares
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;
        private readonly RequestDelegate _next;

        public GlobalErrorHandlingMiddleware(RequestDelegate next,ILogger<GlobalErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                await HandlingNotFoundErrorAsync(context);

            }catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
                await HandlingErrorAsync(context, ex);
            }
            
        }

        private static async Task HandlingErrorAsync(HttpContext context,Exception ex)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorDetails
            {
                StatusCode = ex switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    MoneyInvalidOperationException => StatusCodes.Status400BadRequest,
                    BadRequestException => StatusCodes.Status400BadRequest,
                    UserNotFoundNullException => StatusCodes.Status404NotFound,
                    UnAuthorizedException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError
                },
                Message = ex switch
                {
                    NotFoundException or
                    MoneyInvalidOperationException or
                    BadRequestException or
                    UserNotFoundNullException or
                    UnAuthorizedException => ex.Message,
                    _ => "Internal Server Error. Please try again later."
                }

            };

            context.Response.StatusCode = response.StatusCode;
            await context.Response.WriteAsJsonAsync(response);
            
        }

        private static async Task HandlingNotFoundErrorAsync(HttpContext context)
        {
            if (context.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                context.Response.ContentType = "application/json";
                var respone = new ErrorDetails()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"End point {context.Request.Path} is not found"
                };
                await context.Response.WriteAsJsonAsync(respone);
            }
        }
    }
}
