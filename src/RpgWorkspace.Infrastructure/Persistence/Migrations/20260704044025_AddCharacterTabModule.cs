using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterTabModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "character_tabs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_character_tabs", x => x.id);
                    table.ForeignKey(
                        name: "FK_character_tabs_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_tab_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_tab_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_character_tab_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_character_tab_entries_character_tabs_character_tab_id",
                        column: x => x.character_tab_id,
                        principalTable: "character_tabs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_character_tab_entries_character_tab_id",
                table: "character_tab_entries",
                column: "character_tab_id");

            migrationBuilder.CreateIndex(
                name: "IX_character_tabs_character_id",
                table: "character_tabs",
                column: "character_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_tab_entries");

            migrationBuilder.DropTable(
                name: "character_tabs");
        }
    }
}
