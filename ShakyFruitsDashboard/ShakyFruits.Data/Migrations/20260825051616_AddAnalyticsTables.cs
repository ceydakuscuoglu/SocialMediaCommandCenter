using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShakyFruits.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublishedVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VideoGenerationId = table.Column<int>(type: "int", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    PostUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublishedVideos_VideoGenerations_VideoGenerationId",
                        column: x => x.VideoGenerationId,
                        principalTable: "VideoGenerations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublishedVideoId = table.Column<int>(type: "int", nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    Likes = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<int>(type: "int", nullable: false),
                    Shares = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAnalytics_PublishedVideos_PublishedVideoId",
                        column: x => x.PublishedVideoId,
                        principalTable: "PublishedVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublishedVideos_VideoGenerationId",
                table: "PublishedVideos",
                column: "VideoGenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalytics_PublishedVideoId",
                table: "VideoAnalytics",
                column: "PublishedVideoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoAnalytics");

            migrationBuilder.DropTable(
                name: "PublishedVideos");
        }
    }
}
