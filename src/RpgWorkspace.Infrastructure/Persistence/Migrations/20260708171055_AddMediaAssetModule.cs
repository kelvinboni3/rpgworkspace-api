using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: hand-edited after scaffolding — do NOT drop portrait_url / book_volumes'
            // original_file_name/stored_file_name/file_size_bytes here. Those columns hold the
            // only copy of pre-existing base64 images and the link to on-disk PDFs; they must
            // survive until a one-off data-migration script has copied everything into
            // media_assets and populated the new *_asset_id columns. A follow-up migration
            // (DropLegacyMediaColumns) removes them once that's verified.
            migrationBuilder.AddColumn<Guid>(
                name: "portrait_asset_id",
                table: "characters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "image_asset_id",
                table: "character_tab_blocks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "media_asset_id",
                table: "book_volumes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<byte[]>(type: "bytea", nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_assets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_characters_portrait_asset_id",
                table: "characters",
                column: "portrait_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_character_tab_blocks_image_asset_id",
                table: "character_tab_blocks",
                column: "image_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_book_volumes_media_asset_id",
                table: "book_volumes",
                column: "media_asset_id");

            migrationBuilder.AddForeignKey(
                name: "FK_book_volumes_media_assets_media_asset_id",
                table: "book_volumes",
                column: "media_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_character_tab_blocks_media_assets_image_asset_id",
                table: "character_tab_blocks",
                column: "image_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_characters_media_assets_portrait_asset_id",
                table: "characters",
                column: "portrait_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_book_volumes_media_assets_media_asset_id",
                table: "book_volumes");

            migrationBuilder.DropForeignKey(
                name: "FK_character_tab_blocks_media_assets_image_asset_id",
                table: "character_tab_blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_characters_media_assets_portrait_asset_id",
                table: "characters");

            migrationBuilder.DropTable(
                name: "media_assets");

            migrationBuilder.DropIndex(
                name: "IX_characters_portrait_asset_id",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "IX_character_tab_blocks_image_asset_id",
                table: "character_tab_blocks");

            migrationBuilder.DropIndex(
                name: "IX_book_volumes_media_asset_id",
                table: "book_volumes");

            migrationBuilder.DropColumn(
                name: "portrait_asset_id",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "image_asset_id",
                table: "character_tab_blocks");

            migrationBuilder.DropColumn(
                name: "media_asset_id",
                table: "book_volumes");
        }
    }
}
