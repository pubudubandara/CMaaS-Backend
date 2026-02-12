using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IJwtTokenService
    {
        // Generates a JWT token for the authenticated user
        string GenerateToken(User user);
    }
}
