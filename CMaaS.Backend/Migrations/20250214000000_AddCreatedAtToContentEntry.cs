using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMaaS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToContentEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContentEntries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 14, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContentEntries");
        }
    }
}
