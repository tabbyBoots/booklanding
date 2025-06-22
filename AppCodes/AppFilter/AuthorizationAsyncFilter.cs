using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace mvcDapper3.AppCodes.AppFilter
{
    public class AuthorizationAsyncFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await context.HttpContext.Response.WriteAsync($"{GetType().Name} in. \r\n");
        }
    }
}