using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTagModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_tags_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "location_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_location_tags_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_location_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "narrative_item_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    narrative_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_narrative_item_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_narrative_item_tags_narrative_items_narrative_item_id",
                        column: x => x.narrative_item_id,
                        principalTable: "narrative_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_narrative_item_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "npc_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    npc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npc_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_npc_tags_npcs_npc_id",
                        column: x => x.npc_id,
                        principalTable: "npcs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_npc_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operation_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_operation_tags_operations_operation_id",
                        column: x => x.operation_id,
                        principalTable: "operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_operation_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_note_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_note_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_player_note_tags_player_notes_player_note_id",
                        column: x => x.player_note_id,
                        principalTable: "player_notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_note_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quest_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quest_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_quest_tags_quests_quest_id",
                        column: x => x.quest_id,
                        principalTable: "quests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quest_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_session_tags_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_session_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "theory_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    theory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_theory_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_theory_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_theory_tags_theories_theory_id",
                        column: x => x.theory_id,
                        principalTable: "theories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wiki_page_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wiki_page_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wiki_page_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_wiki_page_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wiki_page_tags_wiki_pages_wiki_page_id",
                        column: x => x.wiki_page_id,
                        principalTable: "wiki_pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "world_library_item_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_library_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_world_library_item_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_world_library_item_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_world_library_item_tags_world_library_items_world_library_i~",
                        column: x => x.world_library_item_id,
                        principalTable: "world_library_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_location_tags_location_id_tag_id",
                table: "location_tags",
                columns: new[] { "location_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_location_tags_tag_id",
                table: "location_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_narrative_item_tags_narrative_item_id_tag_id",
                table: "narrative_item_tags",
                columns: new[] { "narrative_item_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_narrative_item_tags_tag_id",
                table: "narrative_item_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_npc_tags_npc_id_tag_id",
                table: "npc_tags",
                columns: new[] { "npc_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_npc_tags_tag_id",
                table: "npc_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_operation_tags_operation_id_tag_id",
                table: "operation_tags",
                columns: new[] { "operation_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operation_tags_tag_id",
                table: "operation_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_note_tags_player_note_id_tag_id",
                table: "player_note_tags",
                columns: new[] { "player_note_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_note_tags_tag_id",
                table: "player_note_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_quest_tags_quest_id_tag_id",
                table: "quest_tags",
                columns: new[] { "quest_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quest_tags_tag_id",
                table: "quest_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_tags_session_id_tag_id",
                table: "session_tags",
                columns: new[] { "session_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_tags_tag_id",
                table: "session_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_tags_campaign_id_name",
                table: "tags",
                columns: new[] { "campaign_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_theory_tags_tag_id",
                table: "theory_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_theory_tags_theory_id_tag_id",
                table: "theory_tags",
                columns: new[] { "theory_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wiki_page_tags_tag_id",
                table: "wiki_page_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_wiki_page_tags_wiki_page_id_tag_id",
                table: "wiki_page_tags",
                columns: new[] { "wiki_page_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_world_library_item_tags_tag_id",
                table: "world_library_item_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_world_library_item_tags_world_library_item_id_tag_id",
                table: "world_library_item_tags",
                columns: new[] { "world_library_item_id", "tag_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "location_tags");

            migrationBuilder.DropTable(
                name: "narrative_item_tags");

            migrationBuilder.DropTable(
                name: "npc_tags");

            migrationBuilder.DropTable(
                name: "operation_tags");

            migrationBuilder.DropTable(
                name: "player_note_tags");

            migrationBuilder.DropTable(
                name: "quest_tags");

            migrationBuilder.DropTable(
                name: "session_tags");

            migrationBuilder.DropTable(
                name: "theory_tags");

            migrationBuilder.DropTable(
                name: "wiki_page_tags");

            migrationBuilder.DropTable(
                name: "world_library_item_tags");

            migrationBuilder.DropTable(
                name: "tags");
        }
    }
}
