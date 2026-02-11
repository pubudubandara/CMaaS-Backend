namespace CMaaS.Backend.Dtos
{
    public class RegisterRequestDto
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string AdminName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;


    }
}
