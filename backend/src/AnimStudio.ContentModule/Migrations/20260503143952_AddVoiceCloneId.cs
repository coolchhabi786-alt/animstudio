using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.ContentModule.Migrations
{
    /// <inheritdoc />
    public partial class AddVoiceCloneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VoiceAssignments_EpisodeId_CharacterId",
                schema: "content",
                table: "VoiceAssignments");

            migrationBuilder.EnsureSchema(
                name: "timeline");

            migrationBuilder.AddColumn<string>(
                name: "VoiceCloneId",
                schema: "content",
                table: "VoiceAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrainingStatus",
                schema: "content",
                table: "Characters",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Draft");

            migrationBuilder.CreateTable(
                name: "EpisodeTimelines",
                schema: "timeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    Fps = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeTimelines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimelineTextOverlays",
                schema: "timeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimelineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSizePixels = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    StartMs = table.Column<long>(type: "bigint", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Animation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ZIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineTextOverlays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimelineTextOverlays_EpisodeTimelines_TimelineId",
                        column: x => x.TimelineId,
                        principalSchema: "timeline",
                        principalTable: "EpisodeTimelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimelineTracks",
                schema: "timeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimelineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsMuted = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    VolumePercent = table.Column<int>(type: "int", nullable: true),
                    AutoDuck = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimelineTracks_EpisodeTimelines_TimelineId",
                        column: x => x.TimelineId,
                        principalSchema: "timeline",
                        principalTable: "EpisodeTimelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimelineClips",
                schema: "timeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClipType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartMs = table.Column<long>(type: "bigint", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SceneNumber = table.Column<int>(type: "int", nullable: true),
                    ShotIndex = table.Column<int>(type: "int", nullable: true),
                    ClipUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    TransitionIn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AudioUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    VolumePercent = table.Column<int>(type: "int", nullable: true),
                    FadeInMs = table.Column<int>(type: "int", nullable: true),
                    FadeOutMs = table.Column<int>(type: "int", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FontSize = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Animation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineClips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimelineClips_TimelineTracks_TrackId",
                        column: x => x.TrackId,
                        principalSchema: "timeline",
                        principalTable: "TimelineTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoiceAssignments_EpisodeId_CharacterId",
                schema: "content",
                table: "VoiceAssignments",
                columns: new[] { "EpisodeId", "CharacterId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeTimelines_EpisodeId",
                schema: "timeline",
                table: "EpisodeTimelines",
                column: "EpisodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimelineClips_TrackId",
                schema: "timeline",
                table: "TimelineClips",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineTextOverlays_TimelineId",
                schema: "timeline",
                table: "TimelineTextOverlays",
                column: "TimelineId");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineTracks_TimelineId",
                schema: "timeline",
                table: "TimelineTracks",
                column: "TimelineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimelineClips",
                schema: "timeline");

            migrationBuilder.DropTable(
                name: "TimelineTextOverlays",
                schema: "timeline");

            migrationBuilder.DropTable(
                name: "TimelineTracks",
                schema: "timeline");

            migrationBuilder.DropTable(
                name: "EpisodeTimelines",
                schema: "timeline");

            migrationBuilder.DropIndex(
                name: "IX_VoiceAssignments_EpisodeId_CharacterId",
                schema: "content",
                table: "VoiceAssignments");

            migrationBuilder.DropColumn(
                name: "VoiceCloneId",
                schema: "content",
                table: "VoiceAssignments");

            migrationBuilder.AlterColumn<string>(
                name: "TrainingStatus",
                schema: "content",
                table: "Characters",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateIndex(
                name: "IX_VoiceAssignments_EpisodeId_CharacterId",
                schema: "content",
                table: "VoiceAssignments",
                columns: new[] { "EpisodeId", "CharacterId" },
                unique: true);
        }
    }
}
