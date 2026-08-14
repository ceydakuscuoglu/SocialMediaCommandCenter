using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShakyFruits.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVideoGenerationRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoGenerations_ReferenceVideos_ReferenceVideoId",
                table: "VideoGenerations");

            migrationBuilder.AlterColumn<int>(
                name: "ReferenceVideoId",
                table: "VideoGenerations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AppliedPrompt",
                table: "VideoGenerations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "VideoGenerations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecreate",
                table: "VideoGenerations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TargetModel",
                table: "VideoGenerations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetResolution",
                table: "VideoGenerations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetUrl",
                table: "VideoGenerations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoGenerations_ReferenceVideos_ReferenceVideoId",
                table: "VideoGenerations",
                column: "ReferenceVideoId",
                principalTable: "ReferenceVideos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoGenerations_ReferenceVideos_ReferenceVideoId",
                table: "VideoGenerations");

            migrationBuilder.DropColumn(
                name: "AppliedPrompt",
                table: "VideoGenerations");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "VideoGenerations");

            migrationBuilder.DropColumn(
                name: "IsRecreate",
                table: "VideoGenerations");

            migrationBuilder.DropColumn(
                name: "TargetModel",
                table: "VideoGenerations");

            migrationBuilder.DropColumn(
                name: "TargetResolution",
                table: "VideoGenerations");

            migrationBuilder.DropColumn(
                name: "TargetUrl",
                table: "VideoGenerations");

            migrationBuilder.AlterColumn<int>(
                name: "ReferenceVideoId",
                table: "VideoGenerations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoGenerations_ReferenceVideos_ReferenceVideoId",
                table: "VideoGenerations",
                column: "ReferenceVideoId",
                principalTable: "ReferenceVideos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
