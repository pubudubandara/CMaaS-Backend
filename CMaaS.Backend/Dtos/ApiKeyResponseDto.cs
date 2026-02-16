namespace CMaaS.Backend.Dtos
{
    public class ApiKeyResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
