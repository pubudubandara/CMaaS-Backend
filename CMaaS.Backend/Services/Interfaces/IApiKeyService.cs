using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IApiKeyService
    {
        /// <summary>
        /// Creates a new API key for the authenticated tenant
        /// </summary>
        /// <param name="request">API key creation request with name</param>
        /// <returns>Created API key (with full key visible)</returns>
        Task<ServiceResult<CreateApiKeyResponseDto>> CreateApiKeyAsync(CreateApiKeyRequestDto request);

        /// <summary>
        /// Gets all API keys for the authenticated tenant
        /// </summary>
        /// <returns>List of API keys (without the full key value)</returns>
        Task<ServiceResult<List<ApiKeyResponseDto>>> GetAllApiKeysAsync();

        /// <summary>
        /// Gets a specific API key by ID for the authenticated tenant
        /// </summary>
        /// <param name="id">API key ID</param>
        /// <returns>API key details (without the full key value)</returns>
        Task<ServiceResult<ApiKeyResponseDto>> GetApiKeyByIdAsync(int id);

        /// <summary>
        /// Deletes an API key by ID
        /// </summary>
        /// <param name="id">API key ID</param>
        /// <returns>Success or failure</returns>
        Task<ServiceResult<bool>> DeleteApiKeyAsync(int id);
    }
}
