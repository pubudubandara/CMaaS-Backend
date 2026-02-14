namespace CMaaS.Backend.Dtos
{
    public class CreateApiKeyResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty; // Show full key only once
        public DateTime CreatedAt { get; set; }

        public CreateApiKeyResponseDto()
        {
        }

        public CreateApiKeyResponseDto(int id, string name, string key, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Key = key;
            CreatedAt = createdAt;
        }
    }
}
