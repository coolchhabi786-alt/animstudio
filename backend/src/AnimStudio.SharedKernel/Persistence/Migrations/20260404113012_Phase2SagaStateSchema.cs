using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.SharedKernel.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2SagaStateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EpisodeSagaStates",
                schema: "shared",
                table: "EpisodeSagaStates");

            migrationBuilder.DropIndex(
                name: "IX_EpisodeSagaStates_Status",
                schema: "shared",
                table: "EpisodeSagaStates");

            migrationBuilder.DropColumn(
                name: "Data",
                schema: "shared",
                table: "EpisodeSagaStates");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "shared",
                table: "EpisodeSagaStates");

            migrationBuilder.RenameTable(
                name: "EpisodeSagaStates",
                schema: "shared",
                newName: "SagaStates",
                newSchema: "shared");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                schema: "shared",
                table: "SagaStates",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<string>(
                name: "CurrentStage",
                schema: "shared",
                table: "SagaStates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EpisodeId",
                schema: "shared",
                table: "SagaStates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsCompensating",
                schema: "shared",
                table: "SagaStates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                schema: "shared",
                table: "SagaStates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "shared",
                table: "SagaStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                schema: "shared",
                table: "SagaStates",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SagaStates",
                schema: "shared",
                table: "SagaStates",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SagaStates_EpisodeId",
                schema: "shared",
                table: "SagaStates",
                column: "EpisodeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SagaStates",
                schema: "shared",
                table: "SagaStates");

            migrationBuilder.DropIndex(
                name: "IX_SagaStates_EpisodeId",
                schema: "shared",
                table: "SagaStates");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                schema: "shared",
                table: "SagaStates");

            migrationBuilder.DropColumn(
                name: "EpisodeId",
                schema: "shared",
                table: "SagaStates");

            migrationBuilder.DropColumn(
                name: "IsCompensating",
                schema: "shared",
                table: "SagaStates");

            migrationBuilder.DropColumn(
                name: "LastError",
                schema: "shared",
                table: "SagaStates");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "shared",
                table: "SagaStates");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                schema: "shared",
                table: "SagaStates");

            migrationBuilder.RenameTable(
                name: "SagaStates",
                schema: "shared",
                newName: "EpisodeSagaStates",
                newSchema: "shared");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "shared",
                table: "EpisodeSagaStates",
                newName: "LastUpdated");

            migrationBuilder.AddColumn<string>(
                name: "Data",
                schema: "shared",
                table: "EpisodeSagaStates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "shared",
                table: "EpisodeSagaStates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EpisodeSagaStates",
                schema: "shared",
                table: "EpisodeSagaStates",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeSagaStates_Status",
                schema: "shared",
                table: "EpisodeSagaStates",
                column: "Status");
        }
    }
}
