using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiUsageModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    call_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_usages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_usages_user_id_period",
                table: "ai_usages",
                columns: new[] { "user_id", "period" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_usages");
        }
    }
}
