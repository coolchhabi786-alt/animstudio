using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.ContentModule.Migrations
{
    /// <inheritdoc />
    public partial class AddEpisodeCharacterPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CharacterPreferences",
                schema: "content",
                table: "Episodes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharacterPreferences",
                schema: "content",
                table: "Episodes");
        }
    }
}
