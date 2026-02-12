namespace CMaaS.Backend.Dtos
{
    public class RegisterResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string ApiKey { get; set; } = string.Empty;
    }
}
