using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMaaS.Backend.Services.Implementations
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _context;

        public TenantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<List<Tenant>>> GetAllTenantsAsync()
        {
            try
            {
                var tenants = await _context.Tenants.ToListAsync();
                return ServiceResult<List<Tenant>>.Success(tenants);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Tenant>>.Failure($"Failed to retrieve tenants: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Tenant>> CreateTenantAsync(Tenant tenant)
        {
            // Validation
            if (tenant == null)
            {
                return ServiceResult<Tenant>.Failure("Tenant is required.");
            }

            if (string.IsNullOrWhiteSpace(tenant.Name))
            {
                return ServiceResult<Tenant>.Failure("Tenant name is required.");
            }

            // Check if tenant with same name already exists
            var nameExists = await _context.Tenants.AnyAsync(t => t.Name == tenant.Name);
            if (nameExists)
            {
                return ServiceResult<Tenant>.Failure($"A tenant with name '{tenant.Name}' already exists.");
            }

            // Set default values if not provided
            if (tenant.PlanType == 0)
            {
                tenant.PlanType = SubscriptionPlan.Free;
            }

            if (tenant.CreatedAt == default)
            {
                tenant.CreatedAt = DateTime.UtcNow;
            }

            // Note: API key generation is now handled via IApiKeyService.CreateApiKeyAsync()
            // No automatic API key is generated here anymore

            try
            {
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                return ServiceResult<Tenant>.Success(tenant);
            }
            catch (Exception ex)
            {
                return ServiceResult<Tenant>.Failure($"Failed to create tenant: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteTenantAsync(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    return ServiceResult<bool>.Failure("Tenant not found.");
                }

                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Failed to delete tenant: {ex.Message}");
            }
        }

        public async Task<int?> GetTenantIdByApiKey(string apiKey)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.ApiKey == apiKey);
            return tenant?.Id;
        }
    }
}
