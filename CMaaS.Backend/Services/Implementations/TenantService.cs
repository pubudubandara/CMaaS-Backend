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
                throw new Exception($"Failed to retrieve tenants: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Tenant>> CreateTenantAsync(Tenant tenant)
        {
            // Validation
            if (tenant == null)
            {
                throw new ArgumentException("Tenant is required.");
            }

            if (string.IsNullOrWhiteSpace(tenant.Name))
            {
                throw new ArgumentException("Tenant name is required.");
            }

            // Check if tenant with same name already exists
            var nameExists = await _context.Tenants.AnyAsync(t => t.Name == tenant.Name);
            if (nameExists)
            {
                throw new ArgumentException($"A tenant with name '{tenant.Name}' already exists.");
            }

            // Set default values if not provided
            if (string.IsNullOrWhiteSpace(tenant.ApiKey))
            {
                tenant.ApiKey = Guid.NewGuid().ToString("N");
            }

            if (tenant.PlanType == 0)
            {
                tenant.PlanType = SubscriptionPlan.Free;
            }

            if (tenant.CreatedAt == default)
            {
                tenant.CreatedAt = DateTime.UtcNow;
            }

            try
            {
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                return ServiceResult<Tenant>.Success(tenant);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create tenant: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteTenantAsync(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    throw new KeyNotFoundException("Tenant not found.");
                }

                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new Exception($"Failed to delete tenant: {ex.Message}");
            }
        }

        public async Task<int?> GetTenantIdByApiKey(string apiKey)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.ApiKey == apiKey);
            return tenant?.Id;
        }
    }
}
