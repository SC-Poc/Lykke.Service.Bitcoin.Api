using Microsoft.AspNetCore.Builder;

namespace Lykke.Service.Bitcoin.Api.Middleware
{
    public static class MiddlewareApplicationBuilderExtensions
    {
        public static void UseCustomErrorHandligMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<CustomGlobalErrorHandlerMiddleware>();
        }
    }
}
