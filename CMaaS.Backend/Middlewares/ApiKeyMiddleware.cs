using CMaaS.Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CMaaS.Backend.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
        {
            // 1. Check if the API Key exists in the header
            if (context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
            {
                // Need to check the database, so create a scope
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // 2. Find the tenant using the API Key
                    var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.ApiKey == extractedApiKey.ToString());

                    if (tenant != null)
                    {
                        // 3. If valid, create a fake User (Principal) and set it in the system
                        var claims = new List<Claim>
                        {
                            new Claim("TenantId", tenant.Id.ToString()), // Tenant ID as a Claim
                            new Claim(ClaimTypes.Role, "Viewer") // Having an API Key means they're like an API User
                        };

                        var identity = new ClaimsIdentity(claims, "ApiKey");
                        context.User = new ClaimsPrincipal(identity); // <--- This is the most important part!
                    }
                }
            }

            // Send to the next step (JWT Middleware or Controller)
            await _next(context);
        }
    }
}
