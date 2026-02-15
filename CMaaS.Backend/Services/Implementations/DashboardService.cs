using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMaaS.Backend.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContext;

        public DashboardService(AppDbContext context, IUserContextService userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<ServiceResult<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            // Get tenant ID from authenticated user
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null)
            {
                return ServiceResult<DashboardStatsDto>.Failure("Authentication required.");
            }

            try
            {
                // 1. Get total content types for this tenant
                var totalContentTypes = await _context.ContentTypes
                    .CountAsync(ct => ct.TenantId == tenantId);

                // 2. Get total entries for this tenant
                var totalEntries = await _context.ContentEntries
                    .CountAsync(ce => ce.TenantId == tenantId);

                // 3. Get total API keys for this tenant
                var totalApiKeys = await _context.ApiKeys
                    .CountAsync(ak => ak.TenantId == tenantId);

                // 4. Get recent entries (last 10) with their content type names
                var recentEntries = await _context.ContentEntries
                    .Where(ce => ce.TenantId == tenantId)
                    .OrderByDescending(ce => ce.Id)
                    .Take(10)
                    .Select(ce => new RecentEntryDto
                    {
                        Id = ce.Id,
                        TypeName = ce.ContentType!.Name,
                        CreatedAt = ce.CreatedAt
                    })
                    .ToListAsync();

                // Create the dashboard stats DTO
                var dashboardStats = new DashboardStatsDto
                {
                    TotalContentTypes = totalContentTypes,
                    TotalEntries = totalEntries,
                    TotalApiKeys = totalApiKeys,
                    RecentEntries = recentEntries
                };

                return ServiceResult<DashboardStatsDto>.Success(dashboardStats);
            }
            catch (Exception ex)
            {
                return ServiceResult<DashboardStatsDto>.Failure($"Failed to retrieve dashboard statistics: {ex.Message}");
            }
        }
    }
}
