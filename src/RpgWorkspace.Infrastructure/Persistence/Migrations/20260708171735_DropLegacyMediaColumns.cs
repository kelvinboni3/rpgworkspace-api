using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyMediaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // By this point AddMediaAssetModule's data has already been backfilled by the
            // one-off `dotnet run -- --migrate-media` script (portraits, block images, book
            // volume PDFs all copied into media_assets) — safe to drop the old columns now.
            migrationBuilder.DropColumn(
                name: "portrait_url",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "original_file_name",
                table: "book_volumes");

            migrationBuilder.DropColumn(
                name: "stored_file_name",
                table: "book_volumes");

            migrationBuilder.DropColumn(
                name: "file_size_bytes",
                table: "book_volumes");

            migrationBuilder.AlterColumn<Guid>(
                name: "media_asset_id",
                table: "book_volumes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "media_asset_id",
                table: "book_volumes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "portrait_url",
                table: "characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "original_file_name",
                table: "book_volumes",
                type: "character varying(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "stored_file_name",
                table: "book_volumes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "file_size_bytes",
                table: "book_volumes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
