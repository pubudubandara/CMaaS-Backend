using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IAuthService
    {
        // Registers a new company (tenant) with an admin user
        Task<ServiceResult<RegisterResponseDto>> RegisterCompanyAsync(RegisterRequestDto request);


        // Authenticates a user and generates a JWT token
        Task<ServiceResult<string>> LoginAsync(UserDto request);
    }
}
