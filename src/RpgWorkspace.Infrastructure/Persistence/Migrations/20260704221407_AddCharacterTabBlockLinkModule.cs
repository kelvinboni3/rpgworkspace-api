using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterTabBlockLinkModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "character_tab_block_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_block_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_block_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_character_tab_block_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_character_tab_block_links_character_tab_blocks_source_block~",
                        column: x => x.source_block_id,
                        principalTable: "character_tab_blocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_character_tab_block_links_character_tab_blocks_target_block~",
                        column: x => x.target_block_id,
                        principalTable: "character_tab_blocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_character_tab_block_links_source_block_id_target_block_id",
                table: "character_tab_block_links",
                columns: new[] { "source_block_id", "target_block_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_character_tab_block_links_target_block_id",
                table: "character_tab_block_links",
                column: "target_block_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_tab_block_links");
        }
    }
}
