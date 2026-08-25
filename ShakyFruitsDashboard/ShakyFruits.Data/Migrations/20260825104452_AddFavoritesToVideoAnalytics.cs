using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShakyFruits.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoritesToVideoAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Favorites",
                table: "VideoAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Favorites",
                table: "VideoAnalytics");
        }
    }
}
