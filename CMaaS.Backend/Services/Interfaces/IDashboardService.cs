using CMaaS.Backend.Dtos;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Gets dashboard statistics for the authenticated tenant
        /// Including total content types, entries, API keys, and recent entries
        /// </summary>
        Task<ServiceResult<DashboardStatsDto>> GetDashboardStatsAsync();
    }
}
