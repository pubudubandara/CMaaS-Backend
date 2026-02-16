using CMaaS.Backend.Data;
using CMaaS.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CMaaS.Backend.Data
{
    public static class DataSeeder
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // Sample Tenants
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant
                {
                    Id = 1,
                    Name = "Sample Tenant 1",
                    ApiKey = "983d308bd9f340df956c8fedcdf9476c",
                    PlanType = SubscriptionPlan.Free,
                    CreatedAt = DateTime.UtcNow
                },
                new Tenant
                {
                    Id = 2,
                    Name = "Sample Tenant 2",
                    ApiKey = "abc123def456ghi789jkl012mno345",
                    PlanType = SubscriptionPlan.Pro,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Sample Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin User",
                    Email = "admin@sampletenant1.com",
                    PasswordHash = "$2a$11$examplehashedpassword", // Use a real hash in production
                    TenantId = 1,
                    Role = UserRole.Admin
                },
                new User
                {
                    Id = 2,
                    FullName = "Regular User",
                    Email = "user@sampletenant1.com",
                    PasswordHash = "$2a$11$examplehashedpassword",
                    TenantId = 1,
                    Role = UserRole.User
                },
                new User
                {
                    Id = 3,
                    FullName = "Admin User 2",
                    Email = "admin@sampletenant2.com",
                    PasswordHash = "$2a$11$examplehashedpassword",
                    TenantId = 2,
                    Role = UserRole.Admin
                }
            );

            // Sample Content Types
            modelBuilder.Entity<ContentType>().HasData(
                new ContentType
                {
                    Id = 1,
                    Name = "Product",
                    Schema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"description\":{\"type\":\"string\"},\"price\":{\"type\":\"number\"},\"category\":{\"type\":\"string\"}},\"required\":[\"name\",\"price\"]}"),
                    TenantId = 1
                },
                new ContentType
                {
                    Id = 2,
                    Name = "BlogPost",
                    Schema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\"},\"content\":{\"type\":\"string\"},\"author\":{\"type\":\"string\"},\"publishDate\":{\"type\":\"string\",\"format\":\"date\"}},\"required\":[\"title\",\"content\"]}"),
                    TenantId = 1
                },
                new ContentType
                {
                    Id = 3,
                    Name = "Product",
                    Schema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"price\":{\"type\":\"number\"},\"stock\":{\"type\":\"integer\"}},\"required\":[\"name\"]}"),
                    TenantId = 2
                }
            );

            // Sample Content Entries
            modelBuilder.Entity<ContentEntry>().HasData(
                new ContentEntry
                {
                    Id = 1,
                    Data = JsonDocument.Parse("{\"name\":\"Laptop\",\"description\":\"High-performance laptop\",\"price\":999.99,\"category\":\"Electronics\"}"),
                    ContentTypeId = 1,
                    TenantId = 1
                },
                new ContentEntry
                {
                    Id = 2,
                    Data = JsonDocument.Parse("{\"name\":\"Book\",\"description\":\"Programming guide\",\"price\":29.99,\"category\":\"Education\"}"),
                    ContentTypeId = 1,
                    TenantId = 1
                },
                new ContentEntry
                {
                    Id = 3,
                    Data = JsonDocument.Parse("{\"title\":\"Getting Started with CMaaS\",\"content\":\"This is a sample blog post about CMaaS.\",\"author\":\"Admin\",\"publishDate\":\"2023-01-01\"}"),
                    ContentTypeId = 2,
                    TenantId = 1
                },
                new ContentEntry
                {
                    Id = 4,
                    Data = JsonDocument.Parse("{\"name\":\"Tablet\",\"price\":299.99,\"stock\":50}"),
                    ContentTypeId = 3,
                    TenantId = 2
                }
            );
        }
    }
}