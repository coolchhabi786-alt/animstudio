using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.ContentModule.Migrations;

/// <inheritdoc />
public partial class Phase4Characters : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── Characters table ───────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Characters",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                StyleDna = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                LoraWeightsUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                TriggerWord = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                TrainingStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                TrainingProgressPercent = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                CreditsCost = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Characters", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Characters_TeamId",
            schema: "content",
            table: "Characters",
            column: "TeamId");

        // ── EpisodeCharacters join table ───────────────────────────────────
        migrationBuilder.CreateTable(
            name: "EpisodeCharacters",
            schema: "content",
            columns: table => new
            {
                EpisodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AttachedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EpisodeCharacters", x => new { x.EpisodeId, x.CharacterId });
                table.ForeignKey(
                    name: "FK_EpisodeCharacters_Episodes_EpisodeId",
                    column: x => x.EpisodeId,
                    principalSchema: "content",
                    principalTable: "Episodes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EpisodeCharacters_Characters_CharacterId",
                    column: x => x.CharacterId,
                    principalSchema: "content",
                    principalTable: "Characters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EpisodeCharacters", schema: "content");
        migrationBuilder.DropTable(name: "Characters", schema: "content");
    }
}
