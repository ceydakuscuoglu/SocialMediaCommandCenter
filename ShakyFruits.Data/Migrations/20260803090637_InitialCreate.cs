using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShakyFruits.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyTrends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrendType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    ViewCount = table.Column<long>(type: "bigint", nullable: false),
                    TrendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTrends", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FruitAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMultipleFruits = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FruitAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FruitTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FruitTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DanceStyle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceVideos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FruitAssetFruitType",
                columns: table => new
                {
                    FruitAssetsId = table.Column<int>(type: "int", nullable: false),
                    FruitsInImageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FruitAssetFruitType", x => new { x.FruitAssetsId, x.FruitsInImageId });
                    table.ForeignKey(
                        name: "FK_FruitAssetFruitType_FruitAssets_FruitAssetsId",
                        column: x => x.FruitAssetsId,
                        principalTable: "FruitAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FruitAssetFruitType_FruitTypes_FruitsInImageId",
                        column: x => x.FruitsInImageId,
                        principalTable: "FruitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoGenerations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FruitAssetId = table.Column<int>(type: "int", nullable: false),
                    ReferenceVideoId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputVideoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiGeneratedCaption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoGenerations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoGenerations_FruitAssets_FruitAssetId",
                        column: x => x.FruitAssetId,
                        principalTable: "FruitAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoGenerations_ReferenceVideos_ReferenceVideoId",
                        column: x => x.ReferenceVideoId,
                        principalTable: "ReferenceVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FruitAssetFruitType_FruitsInImageId",
                table: "FruitAssetFruitType",
                column: "FruitsInImageId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoGenerations_FruitAssetId",
                table: "VideoGenerations",
                column: "FruitAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoGenerations_ReferenceVideoId",
                table: "VideoGenerations",
                column: "ReferenceVideoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyTrends");

            migrationBuilder.DropTable(
                name: "FruitAssetFruitType");

            migrationBuilder.DropTable(
                name: "VideoGenerations");

            migrationBuilder.DropTable(
                name: "FruitTypes");

            migrationBuilder.DropTable(
                name: "FruitAssets");

            migrationBuilder.DropTable(
                name: "ReferenceVideos");
        }
    }
}
