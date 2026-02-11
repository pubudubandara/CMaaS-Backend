namespace CMaaS.Backend.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;

        public SubscriptionPlan PlanType { get; set; } = SubscriptionPlan.Free;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
