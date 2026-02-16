using CMaaS.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CMaaS.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ContentType> ContentTypes { get; set; }
        public DbSet<ContentEntry> ContentEntries { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApiKey -> Tenant relationship
            modelBuilder.Entity<ApiKey>()
                .HasOne(ak => ak.Tenant)
                .WithMany(t => t.ApiKeys)
                .HasForeignKey(ak => ak.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed sample data
            modelBuilder.Seed();
        }

    }
}
