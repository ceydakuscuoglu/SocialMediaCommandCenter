using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShakyFruits.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLifetimeStatsToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FollowingCount",
                table: "AccountAnalyticsHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LifetimeLikes",
                table: "AccountAnalyticsHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalFollowers",
                table: "AccountAnalyticsHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowingCount",
                table: "AccountAnalyticsHistory");

            migrationBuilder.DropColumn(
                name: "LifetimeLikes",
                table: "AccountAnalyticsHistory");

            migrationBuilder.DropColumn(
                name: "TotalFollowers",
                table: "AccountAnalyticsHistory");
        }
    }
}
