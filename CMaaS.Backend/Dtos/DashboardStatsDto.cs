namespace CMaaS.Backend.Dtos
{
    public class DashboardStatsDto
    {
        public int TotalContentTypes { get; set; }
        public int TotalEntries { get; set; }
        public int TotalApiKeys { get; set; }
        public List<RecentEntryDto> RecentEntries { get; set; } = new List<RecentEntryDto>();
    }
}
