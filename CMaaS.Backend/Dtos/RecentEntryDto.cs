namespace CMaaS.Backend.Dtos
{
    public class RecentEntryDto
    {
        public int Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
