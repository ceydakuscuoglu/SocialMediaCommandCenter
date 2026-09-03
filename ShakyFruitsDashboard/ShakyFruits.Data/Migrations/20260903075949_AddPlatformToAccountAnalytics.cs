using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShakyFruits.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformToAccountAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Platform",
                table: "AccountAnalyticsHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Platform",
                table: "AccountAnalyticsHistory");
        }
    }
}
