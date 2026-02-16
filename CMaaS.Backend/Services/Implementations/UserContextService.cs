using CMaaS.Backend.Services.Interfaces;
using System.Security.Claims;

namespace CMaaS.Backend.Services.Implementations
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetUserId()
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier); // or "id"
            return idClaim != null ? int.Parse(idClaim.Value) : null;
        }

        public int? GetTenantId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                return null; 
            }

            // 
            var claim = user.FindFirst("TenantId");

            // 2. 
            if (claim == null)
            {
                claim = user.FindFirst("tenantId");
            }

            if (claim != null && int.TryParse(claim.Value, out int tenantId))
            {
                return tenantId;
            }

            return null;
        }

        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public string GetAuthenticationMethod()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.AuthenticationType ?? string.Empty;
        }

        public string GetTenantName()
        {
            var tenantNameClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantName");
            return tenantNameClaim?.Value ?? string.Empty;
        }

        public string GetApiKeyName()
        {
            var apiKeyNameClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("ApiKeyName");
            return apiKeyNameClaim?.Value ?? string.Empty;
        }
    }
}