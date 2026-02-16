using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CMaaS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialApiKeysSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    PlanType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Schema = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentTypes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    ContentTypeId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentEntries_ContentTypes_ContentTypeId",
                        column: x => x.ContentTypeId,
                        principalTable: "ContentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "ApiKey", "CreatedAt", "Name", "PlanType" },
                values: new object[,]
                {
                    { 1, "983d308bd9f340df956c8fedcdf9476c", new DateTime(2026, 2, 14, 5, 35, 26, 685, DateTimeKind.Utc).AddTicks(7929), "Sample Tenant 1", 0 },
                    { 2, "abc123def456ghi789jkl012mno345", new DateTime(2026, 2, 14, 5, 35, 26, 685, DateTimeKind.Utc).AddTicks(7931), "Sample Tenant 2", 1 }
                });

            migrationBuilder.InsertData(
                table: "ContentTypes",
                columns: new[] { "Id", "Name", "Schema", "TenantId" },
                values: new object[,]
                {
                    { 1, "Product", System.Text.Json.JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"description\":{\"type\":\"string\"},\"price\":{\"type\":\"number\"},\"category\":{\"type\":\"string\"}},\"required\":[\"name\",\"price\"]}", new System.Text.Json.JsonDocumentOptions()), 1 },
                    { 2, "BlogPost", System.Text.Json.JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\"},\"content\":{\"type\":\"string\"},\"author\":{\"type\":\"string\"},\"publishDate\":{\"type\":\"string\",\"format\":\"date\"}},\"required\":[\"title\",\"content\"]}", new System.Text.Json.JsonDocumentOptions()), 1 },
                    { 3, "Product", System.Text.Json.JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"price\":{\"type\":\"number\"},\"stock\":{\"type\":\"integer\"}},\"required\":[\"name\"]}", new System.Text.Json.JsonDocumentOptions()), 2 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "PasswordHash", "Role", "TenantId" },
                values: new object[,]
                {
                    { 1, "admin@sampletenant1.com", "Admin User", "$2a$11$examplehashedpassword", 0, 1 },
                    { 2, "user@sampletenant1.com", "Regular User", "$2a$11$examplehashedpassword", 1, 1 },
                    { 3, "admin@sampletenant2.com", "Admin User 2", "$2a$11$examplehashedpassword", 0, 2 }
                });

            migrationBuilder.InsertData(
                table: "ContentEntries",
                columns: new[] { "Id", "ContentTypeId", "Data", "TenantId" },
                values: new object[,]
                {
                    { 1, 1, System.Text.Json.JsonDocument.Parse("{\"name\":\"Laptop\",\"description\":\"High-performance laptop\",\"price\":999.99,\"category\":\"Electronics\"}", new System.Text.Json.JsonDocumentOptions()), 1 },
                    { 2, 1, System.Text.Json.JsonDocument.Parse("{\"name\":\"Book\",\"description\":\"Programming guide\",\"price\":29.99,\"category\":\"Education\"}", new System.Text.Json.JsonDocumentOptions()), 1 },
                    { 3, 2, System.Text.Json.JsonDocument.Parse("{\"title\":\"Getting Started with CMaaS\",\"content\":\"This is a sample blog post about CMaaS.\",\"author\":\"Admin\",\"publishDate\":\"2023-01-01\"}", new System.Text.Json.JsonDocumentOptions()), 1 },
                    { 4, 3, System.Text.Json.JsonDocument.Parse("{\"name\":\"Tablet\",\"price\":299.99,\"stock\":50}", new System.Text.Json.JsonDocumentOptions()), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_TenantId",
                table: "ApiKeys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentEntries_ContentTypeId",
                table: "ContentEntries",
                column: "ContentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentEntries_TenantId",
                table: "ContentEntries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentTypes_TenantId",
                table: "ContentTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "ContentEntries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ContentTypes");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
