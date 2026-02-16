using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMaaS.Backend.Services.Implementations
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContext;

        public ApiKeyService(AppDbContext context, IUserContextService userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<ServiceResult<CreateApiKeyResponseDto>> CreateApiKeyAsync(CreateApiKeyRequestDto request)
        {
            // Get tenant ID from authenticated user
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null)
            {
                return ServiceResult<CreateApiKeyResponseDto>.Failure("Authentication required.");
            }

            // Validation
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return ServiceResult<CreateApiKeyResponseDto>.Failure("API key name is required.");
            }

            if (request.Name.Length > 100)
            {
                return ServiceResult<CreateApiKeyResponseDto>.Failure("API key name cannot exceed 100 characters.");
            }

            try
            {
                // Check if name already exists for this tenant
                var nameExists = await _context.ApiKeys
                    .AnyAsync(ak => ak.Name == request.Name && ak.TenantId == tenantId);

                if (nameExists)
                {
                    return ServiceResult<CreateApiKeyResponseDto>.Failure($"An API key with name '{request.Name}' already exists.");
                }

                // Generate a unique API key
                var apiKey = GenerateApiKey();

                // Create the API key record
                var apiKeyRecord = new ApiKey
                {
                    Name = request.Name,
                    Key = HashApiKey(apiKey), // Store hashed version
                    TenantId = tenantId.Value,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ApiKeys.Add(apiKeyRecord);
                await _context.SaveChangesAsync();

                // Return the plain key only once (when created)
                var response = new CreateApiKeyResponseDto(
                    apiKeyRecord.Id,
                    apiKeyRecord.Name,
                    apiKey, // Return plain key (this is the only time it's shown)
                    apiKeyRecord.CreatedAt
                );

                return ServiceResult<CreateApiKeyResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<CreateApiKeyResponseDto>.Failure($"Failed to create API key: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ApiKeyResponseDto>>> GetAllApiKeysAsync()
        {
            // Get tenant ID from authenticated user
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null)
            {
                return ServiceResult<List<ApiKeyResponseDto>>.Failure("Authentication required.");
            }

            try
            {
                var apiKeys = await _context.ApiKeys
                    .Where(ak => ak.TenantId == tenantId)
                    .ToListAsync();

                var response = apiKeys.Select(ak => new ApiKeyResponseDto
                {
                    Id = ak.Id,
                    Name = ak.Name,
                    Key = GetLastFourDigits(ak.Key), // Show last 4 digits only
                    CreatedAt = ak.CreatedAt
                }).ToList();

                return ServiceResult<List<ApiKeyResponseDto>>.Success(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ApiKeyResponseDto>>.Failure($"Failed to retrieve API keys: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ApiKeyResponseDto>> GetApiKeyByIdAsync(int id)
        {
            // Get tenant ID from authenticated user
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null)
            {
                return ServiceResult<ApiKeyResponseDto>.Failure("Authentication required.");
            }

            if (id <= 0)
            {
                return ServiceResult<ApiKeyResponseDto>.Failure("Invalid API key ID.");
            }

            try
            {
                var apiKey = await _context.ApiKeys
                    .FirstOrDefaultAsync(ak => ak.Id == id && ak.TenantId == tenantId);

                if (apiKey == null)
                {
                    return ServiceResult<ApiKeyResponseDto>.Failure("API key not found or access denied.");
                }

                var response = new ApiKeyResponseDto
                {
                    Id = apiKey.Id,
                    Name = apiKey.Name,
                    Key = GetLastFourDigits(apiKey.Key), // Show last 4 digits only
                    CreatedAt = apiKey.CreatedAt
                };

                return ServiceResult<ApiKeyResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<ApiKeyResponseDto>.Failure($"Failed to retrieve API key: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteApiKeyAsync(int id)
        {
            // Get tenant ID from authenticated user
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null)
            {
                return ServiceResult<bool>.Failure("Authentication required.");
            }

            if (id <= 0)
            {
                return ServiceResult<bool>.Failure("Invalid API key ID.");
            }

            try
            {
                var apiKey = await _context.ApiKeys
                    .FirstOrDefaultAsync(ak => ak.Id == id && ak.TenantId == tenantId);

                if (apiKey == null)
                {
                    return ServiceResult<bool>.Failure("API key not found or access denied.");
                }

                _context.ApiKeys.Remove(apiKey);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Failed to delete API key: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a random API key
        /// </summary>
        private string GenerateApiKey()
        {
            // Format: cmaas_[random_string]
            var randomBytes = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var base64Key = Convert.ToBase64String(randomBytes);
            // Replace unsafe characters for URLs
            var urlSafeKey = base64Key.Replace("+", "-").Replace("/", "_").Replace("=", "");
            return $"cmaas_{urlSafeKey}";
        }

        /// <summary>
        /// Hashes the API key using SHA256
        /// </summary>
        private string HashApiKey(string apiKey)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(apiKey));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Returns the last 4 characters of the hashed API key
        /// </summary>
        private string GetLastFourDigits(string hashedKey)
        {
            if (string.IsNullOrEmpty(hashedKey) || hashedKey.Length < 4)
            {
                return "****";
            }
            return hashedKey.Substring(hashedKey.Length - 4);
        }
    }
}
