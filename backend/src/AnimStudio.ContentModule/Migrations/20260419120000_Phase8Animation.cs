using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.ContentModule.Migrations;

/// <inheritdoc />
public partial class Phase8Animation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AnimationJobs",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EpisodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Backend = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                EstimatedCostUsd = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                ActualCostUsd = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AnimationJobs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AnimationClips",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EpisodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SceneNumber = table.Column<int>(type: "int", nullable: false),
                ShotIndex = table.Column<int>(type: "int", nullable: false),
                StoryboardShotId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ClipUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                DurationSeconds = table.Column<double>(type: "float", nullable: true),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AnimationClips", x => x.Id);
                table.ForeignKey(
                    name: "FK_AnimationClips_StoryboardShots_StoryboardShotId",
                    column: x => x.StoryboardShotId,
                    principalSchema: "content",
                    principalTable: "StoryboardShots",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AnimationJobs_EpisodeId",
            schema: "content",
            table: "AnimationJobs",
            column: "EpisodeId");

        migrationBuilder.CreateIndex(
            name: "IX_AnimationJobs_Status",
            schema: "content",
            table: "AnimationJobs",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_AnimationClips_EpisodeId",
            schema: "content",
            table: "AnimationClips",
            column: "EpisodeId");

        migrationBuilder.CreateIndex(
            name: "IX_AnimationClips_EpisodeId_SceneNumber_ShotIndex",
            schema: "content",
            table: "AnimationClips",
            columns: new[] { "EpisodeId", "SceneNumber", "ShotIndex" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AnimationClips_StoryboardShotId",
            schema: "content",
            table: "AnimationClips",
            column: "StoryboardShotId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AnimationClips",
            schema: "content");

        migrationBuilder.DropTable(
            name: "AnimationJobs",
            schema: "content");
    }
}
