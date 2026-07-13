using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterAccentColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "accent_color",
                table: "characters",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "accent_color",
                table: "characters");
        }
    }
}
