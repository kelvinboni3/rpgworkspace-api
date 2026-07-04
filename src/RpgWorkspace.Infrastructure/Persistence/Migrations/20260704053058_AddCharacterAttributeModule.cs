using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterAttributeModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "portrait_url",
                table: "characters",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "character_attributes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_character_attributes", x => x.id);
                    table.ForeignKey(
                        name: "FK_character_attributes_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_character_attributes_character_id",
                table: "character_attributes",
                column: "character_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_attributes");

            migrationBuilder.DropColumn(
                name: "portrait_url",
                table: "characters");
        }
    }
}
