using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface ITenantService
    {

        // Gets all tenants
        Task<ServiceResult<List<Tenant>>> GetAllTenantsAsync();

        // Creates a new tenant
        Task<ServiceResult<Tenant>> CreateTenantAsync(Tenant tenant);

        // Deletes a tenant by id
        Task<ServiceResult<bool>> DeleteTenantAsync(int id);

        // Gets tenant ID by API key
        Task<int?> GetTenantIdByApiKey(string apiKey);
    }
}
