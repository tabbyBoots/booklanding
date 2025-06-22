

namespace mvcDapper3.AppCodes.AppMiddleware
{
    public class AuthenticationRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public AuthenticationRedirectMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication check for login page and other anonymous pages
            if (IsAnonymousPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Check if the request has a JWT cookie
            if (!context.Request.Cookies.TryGetValue("jwtToken", out string token))
            {
                // Redirect to login page if no token
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Get JWTServices from the scoped service provider
            using (var scope = _serviceProvider.CreateScope())
            {
                var jwtServices = scope.ServiceProvider.GetRequiredService<JWTServices>();
                if (!jwtServices.ValidateToken(token))
                {
                    // Redirect to login page if invalid token
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }

        private bool IsAnonymousPath(PathString path)
        {
            // Define paths that don't require authentication
            return path.StartsWithSegments("/Account/Login") ||
                   path.StartsWithSegments("/Account/Register") ||
                   path.StartsWithSegments("/Account/ForgotPassword") ||
                   path.StartsWithSegments("/Account/ResetPassword") ||
                   path.StartsWithSegments("/Account/CaptchaImage") ||
                   path.StartsWithSegments("/Account/RefreshCaptcha") ||
                   path.StartsWithSegments("/Account/RefreshCaptchaImg") ||
                   path.StartsWithSegments("/Account/Activate") ||
                   path.StartsWithSegments("/Account/GoogleLogin") ||
                   path.StartsWithSegments("/Account/GoogleCallback") ||
                   path.StartsWithSegments("/Home/Index") ||
                   path.StartsWithSegments("/Home/Privacy") ||
                   path.StartsWithSegments("/css") ||
                   path.StartsWithSegments("/js") ||
                   path.StartsWithSegments("/lib") ||
                   path.StartsWithSegments("/images") ||
                   path.StartsWithSegments("/favicon.ico") ||
                   path.Value == "/" ||  // Allow access to root path
                   path.Value == "";     // Allow access to empty path
        }
    }

    // Extension method to make it easier to add the middleware to the pipeline
    public static class AuthenticationRedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationRedirect(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationRedirectMiddleware>();
        }
    }
}
