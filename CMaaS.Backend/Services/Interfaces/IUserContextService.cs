namespace CMaaS.Backend.Services.Interfaces
{
    public interface IUserContextService
    {
        int? GetUserId();
        int? GetTenantId();
        string GetUserRole();
        bool IsAuthenticated();
        string GetAuthenticationMethod();
    }
}