using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterVitalsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "hp_current",
                table: "characters",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "hp_max",
                table: "characters",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mp_current",
                table: "characters",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mp_max",
                table: "characters",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hp_current",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "hp_max",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "mp_current",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "mp_max",
                table: "characters");
        }
    }
}
