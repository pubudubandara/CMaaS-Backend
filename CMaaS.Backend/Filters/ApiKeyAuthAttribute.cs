using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CMaaS.Backend.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if the API Key exists in the header
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                // No API key provided, let other authorization handle it (e.g., JWT)
                return;
            }

            var tenantService = context.HttpContext.RequestServices.GetRequiredService<ITenantService>();
            var tenantId = await tenantService.GetTenantIdByApiKey(extractedApiKey!);

            if (tenantId == null)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 403,
                    Content = "Invalid API Key"
                };
                return;
            }

            // Set authenticated user for API key
            var claims = new[] { new Claim("TenantId", tenantId.ToString()) };
            var identity = new ClaimsIdentity(claims, "ApiKey");
            context.HttpContext.User = new ClaimsPrincipal(identity);

            context.HttpContext.Items["TenantId"] = tenantId;
        }
    }
}