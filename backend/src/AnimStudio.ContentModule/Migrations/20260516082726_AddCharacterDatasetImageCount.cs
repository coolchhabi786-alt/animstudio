using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.ContentModule.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterDatasetImageCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DatasetImageCount",
                schema: "content",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatasetImageCount",
                schema: "content",
                table: "Characters");
        }
    }
}
