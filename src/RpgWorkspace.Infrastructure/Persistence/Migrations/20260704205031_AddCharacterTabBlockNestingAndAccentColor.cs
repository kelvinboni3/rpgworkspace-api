using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterTabBlockNestingAndAccentColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "accent_color",
                table: "character_tab_blocks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_block_id",
                table: "character_tab_blocks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_character_tab_blocks_parent_block_id",
                table: "character_tab_blocks",
                column: "parent_block_id");

            migrationBuilder.AddForeignKey(
                name: "FK_character_tab_blocks_character_tab_blocks_parent_block_id",
                table: "character_tab_blocks",
                column: "parent_block_id",
                principalTable: "character_tab_blocks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_character_tab_blocks_character_tab_blocks_parent_block_id",
                table: "character_tab_blocks");

            migrationBuilder.DropIndex(
                name: "IX_character_tab_blocks_parent_block_id",
                table: "character_tab_blocks");

            migrationBuilder.DropColumn(
                name: "accent_color",
                table: "character_tab_blocks");

            migrationBuilder.DropColumn(
                name: "parent_block_id",
                table: "character_tab_blocks");
        }
    }
}
