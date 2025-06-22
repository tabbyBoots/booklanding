using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace mvcDapper3.AppCodes.AppMiddleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request has a JWT cookie
            if (context.Request.Cookies.TryGetValue("jwtToken", out string token))
            {
                // Add the JWT token to the Authorization header
                context.Request.Headers.Append("Authorization", $"Bearer {token}");
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

    // Extension method to make it easier to add the middleware to the pipeline
    public static class JwtCookieMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookieMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }
}
