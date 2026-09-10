using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShakyFruits.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToVideoGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PublishedVideos_VideoGenerationId",
                table: "PublishedVideos");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "VideoGenerations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedVideos_VideoGenerationId",
                table: "PublishedVideos",
                column: "VideoGenerationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PublishedVideos_VideoGenerationId",
                table: "PublishedVideos");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "VideoGenerations");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedVideos_VideoGenerationId",
                table: "PublishedVideos",
                column: "VideoGenerationId");
        }
    }
}
