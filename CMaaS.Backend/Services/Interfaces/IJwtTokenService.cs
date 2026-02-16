using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for the authenticated user
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="tenantName">The tenant/organization name</param>
        /// <returns>JWT token string</returns>
        string GenerateToken(User user, string tenantName = "");
    }
}
