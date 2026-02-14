namespace CMaaS.Backend.Models
{
    public class ApiKey
    {
        public int Id { get; set; }

        /// <summary>
        /// The actual API key value (stored as hashed for security)
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// User-friendly name for the API key
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// When this API key was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Foreign key to Tenant
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Navigation property to Tenant
        /// </summary>
        public Tenant? Tenant { get; set; }
    }
}
