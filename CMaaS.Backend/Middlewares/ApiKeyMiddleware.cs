using CMaaS.Backend.Data;
using CMaaS.Backend.Services.Interfaces;
using System.Security.Claims;

namespace CMaaS.Backend.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Check if request has API Key header
            if (context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
            {
                try
                {
                    // Hash the provided API key
                    var hashedProvidedKey = HashApiKey(extractedApiKey.ToString());
                    
                    // Search for matching API key in database
                    var matchingApiKey = dbContext.ApiKeys
                        .FirstOrDefault(ak => ak.Key == hashedProvidedKey);

                    if (matchingApiKey != null)
                    {
                        // Set up the user context with API key claims
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, "api-key-user"),
                            new Claim("TenantId", matchingApiKey.TenantId.ToString()),
                            new Claim(ClaimTypes.AuthenticationMethod, "ApiKey"),
                            new Claim("ApiKeyId", matchingApiKey.Id.ToString()),
                            new Claim("ApiKeyName", matchingApiKey.Name ?? "Unknown")
                        };

                        var identity = new ClaimsIdentity(claims, "ApiKey");
                        var principal = new ClaimsPrincipal(identity);
                        context.User = principal;

                        _logger.LogInformation($"API Key authentication successful for API Key ID: {matchingApiKey.Id}");
                    }
                    else
                    {
                        _logger.LogWarning("Invalid API Key provided");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Invalid API Key.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating API Key");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("API Key validation failed.");
                    return;
                }
            }

            // Continue to next middleware
            await _next(context);
        }

        // Hash API key using SHA256
        private string HashApiKey(string apiKey)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(apiKey));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
