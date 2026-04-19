using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.ContentModule.Migrations;

/// <inheritdoc />
public partial class Phase7Voice : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VoiceAssignments",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EpisodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                VoiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "en-US"),
                VoiceCloneUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoiceAssignments", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_VoiceAssignments_EpisodeId",
            schema: "content",
            table: "VoiceAssignments",
            column: "EpisodeId");

        migrationBuilder.CreateIndex(
            name: "IX_VoiceAssignments_EpisodeId_CharacterId",
            schema: "content",
            table: "VoiceAssignments",
            columns: new[] { "EpisodeId", "CharacterId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "VoiceAssignments",
            schema: "content");
    }
}
