using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterRetrospective : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "retrospective_text",
                table: "characters",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "retrospective_text",
                table: "characters");
        }
    }
}
