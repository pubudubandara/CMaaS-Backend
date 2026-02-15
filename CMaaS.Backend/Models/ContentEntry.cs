using System.Text.Json;

namespace CMaaS.Backend.Models
{
    public class ContentEntry
    {
        public int Id { get; set; }

        //Store the content data as a JSON document. This allows for flexible and dynamic content entries that can conform to different content type schemas.
        public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");
        public int ContentTypeId { get; set; }
        public ContentType? ContentType { get; set; }

        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public bool IsVisible { get; set; } = true;
        // Timestamp when the content entry was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
