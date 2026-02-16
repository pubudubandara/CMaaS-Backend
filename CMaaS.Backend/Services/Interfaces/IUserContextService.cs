namespace CMaaS.Backend.Services.Interfaces
{
    public interface IUserContextService
    {
        int? GetUserId();
        int? GetTenantId();
        string GetUserRole();
        bool IsAuthenticated();
        string GetAuthenticationMethod();
        
        /// <summary>
        /// Gets the tenant name (organization name) from the current user context
        /// </summary>
        string GetTenantName();
        
        /// <summary>
        /// Gets the API key name (if authenticated with API Key)
        /// </summary>
        string GetApiKeyName();
    }
}