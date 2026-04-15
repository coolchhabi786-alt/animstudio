using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.ContentModule.Migrations;

/// <inheritdoc />
public partial class Phase6Storyboard : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Storyboards",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EpisodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ScreenplayTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                RawJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DirectorNotes = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Storyboards", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "StoryboardShots",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StoryboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SceneNumber = table.Column<int>(type: "int", nullable: false),
                ShotIndex = table.Column<int>(type: "int", nullable: false),
                ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                StyleOverride = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                RegenerationCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StoryboardShots", x => x.Id);
                table.ForeignKey(
                    name: "FK_StoryboardShots_Storyboards_StoryboardId",
                    column: x => x.StoryboardId,
                    principalSchema: "content",
                    principalTable: "Storyboards",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Storyboards_EpisodeId",
            schema: "content",
            table: "Storyboards",
            column: "EpisodeId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StoryboardShots_StoryboardId",
            schema: "content",
            table: "StoryboardShots",
            column: "StoryboardId");

        migrationBuilder.CreateIndex(
            name: "IX_StoryboardShots_StoryboardId_SceneNumber_ShotIndex",
            schema: "content",
            table: "StoryboardShots",
            columns: new[] { "StoryboardId", "SceneNumber", "ShotIndex" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "StoryboardShots",
            schema: "content");

        migrationBuilder.DropTable(
            name: "Storyboards",
            schema: "content");
    }
}
