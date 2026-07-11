using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgWorkspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceStructuredContentWithBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Schema additions first: the data-migration step below needs
            // character_tabs.order and character_tab_blocks to already exist. ---

            migrationBuilder.AddColumn<int>(
                name: "order",
                table: "character_tabs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "character_tab_blocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_tab_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    meta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    payload_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_character_tab_blocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_character_tab_blocks_character_tabs_character_tab_id",
                        column: x => x.character_tab_id,
                        principalTable: "character_tabs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_character_tab_blocks_character_tab_id",
                table: "character_tab_blocks",
                column: "character_tab_id");

            // --- Data migration: convert every row from the structured tables into
            // an equivalent tab + block, before the structured tables are dropped. ---

            migrationBuilder.Sql(
                """
                -- Push pre-existing custom tabs after the new default tabs (order 0-5 below).
                UPDATE character_tabs SET "order" = 100;

                -- Give every character the 6 default tabs the app always shows.
                INSERT INTO character_tabs (id, character_id, name, "order", created_at)
                SELECT gen_random_uuid(), c.id, t.name, t.ord, now()
                FROM characters c
                CROSS JOIN (VALUES
                    ('Status', 0),
                    ('Diário', 1),
                    ('Itens Narrativos', 2),
                    ('Teorias', 3),
                    ('Operações', 4),
                    ('Pessoas', 5)
                ) AS t(name, ord);

                -- character_attributes -> one Card block ("Atributos") per character in Status.
                INSERT INTO character_tab_blocks (id, character_tab_id, type, "order", title, meta, content, payload_json, created_at)
                SELECT gen_random_uuid(), ct.id, 'Card', 0, 'Atributos', NULL, NULL,
                    (SELECT json_agg(json_build_object('k', ca.name, 'v', ca.value) ORDER BY ca.created_at)::text
                     FROM character_attributes ca WHERE ca.character_id = ct.character_id),
                    now()
                FROM character_tabs ct
                WHERE ct.name = 'Status'
                  AND EXISTS (SELECT 1 FROM character_attributes ca2 WHERE ca2.character_id = ct.character_id);

                -- player_notes -> Collapse blocks in Diário.
                INSERT INTO character_tab_blocks (id, character_tab_id, type, "order", title, meta, content, payload_json, created_at, updated_at)
                SELECT gen_random_uuid(), ct.id, 'Collapse',
                    ROW_NUMBER() OVER (PARTITION BY n.character_id ORDER BY n.created_at) - 1,
                    n.title, to_char(n.created_at, 'DD/MM/YYYY'), n.content, NULL, n.created_at, n.updated_at
                FROM player_notes n
                JOIN character_tabs ct ON ct.character_id = n.character_id AND ct.name = 'Diário';

                -- narrative_items -> Collapse blocks in Itens Narrativos.
                INSERT INTO character_tab_blocks (id, character_tab_id, type, "order", title, meta, content, payload_json, created_at, updated_at)
                SELECT gen_random_uuid(), ct.id, 'Collapse',
                    ROW_NUMBER() OVER (PARTITION BY n.character_id ORDER BY n.created_at) - 1,
                    n.name,
                    'Importância: ' || (CASE n.importance
                        WHEN 'Low' THEN 'Baixa' WHEN 'Medium' THEN 'Média'
                        WHEN 'High' THEN 'Alta' WHEN 'Critical' THEN 'Crítica'
                        ELSE n.importance END),
                    concat_ws(E'\n\n',
                        CASE WHEN n.origin IS NOT NULL THEN '**Origem:** ' || n.origin END,
                        n.description,
                        CASE WHEN n.notes IS NOT NULL THEN E'**Notas:**\n' || n.notes END),
                    NULL, n.created_at, n.updated_at
                FROM narrative_items n
                JOIN character_tabs ct ON ct.character_id = n.character_id AND ct.name = 'Itens Narrativos';

                -- theories -> Collapse blocks in Teorias.
                INSERT INTO character_tab_blocks (id, character_tab_id, type, "order", title, meta, content, payload_json, created_at, updated_at)
                SELECT gen_random_uuid(), ct.id, 'Collapse',
                    ROW_NUMBER() OVER (PARTITION BY t.character_id ORDER BY t.created_at) - 1,
                    t.title,
                    (CASE t.status
                        WHEN 'Active' THEN 'Ativa' WHEN 'Confirmed' THEN 'Confirmada'
                        WHEN 'Refuted' THEN 'Refutada' WHEN 'Archived' THEN 'Arquivada'
                        ELSE t.status END) || ' · Confiança: ' || t.confidence || '%',
                    concat_ws(E'\n\n',
                        t.description,
                        CASE WHEN t.evidence IS NOT NULL THEN E'**Evidências:**\n' || t.evidence END),
                    NULL, t.created_at, t.updated_at
                FROM theories t
                JOIN character_tabs ct ON ct.character_id = t.character_id AND ct.name = 'Teorias';

                -- operations -> Collapse blocks in Operações.
                INSERT INTO character_tab_blocks (id, character_tab_id, type, "order", title, meta, content, payload_json, created_at, updated_at)
                SELECT gen_random_uuid(), ct.id, 'Collapse',
                    ROW_NUMBER() OVER (PARTITION BY o.character_id ORDER BY o.created_at) - 1,
                    o.name,
                    (CASE o.status
                        WHEN 'Planned' THEN 'Planejada' WHEN 'InProgress' THEN 'Em andamento'
                        WHEN 'Completed' THEN 'Concluída' WHEN 'Failed' THEN 'Fracassada'
                        WHEN 'Canceled' THEN 'Cancelada' WHEN 'Archived' THEN 'Arquivada'
                        ELSE o.status END),
                    concat_ws(E'\n\n',
                        CASE WHEN o.objective IS NOT NULL THEN E'**Objetivo:**\n' || o.objective END,
                        CASE WHEN o.plan IS NOT NULL THEN E'**Plano:**\n' || o.plan END,
                        CASE WHEN o.required_resources IS NOT NULL THEN E'**Recursos necessários:**\n' || o.required_resources END,
                        CASE WHEN o.risks IS NOT NULL THEN E'**Riscos:**\n' || o.risks END,
                        CASE WHEN o.result IS NOT NULL THEN E'**Resultado:**\n' || o.result END),
                    NULL, o.created_at, o.updated_at
                FROM operations o
                JOIN character_tabs ct ON ct.character_id = o.character_id AND ct.name = 'Operações';

                -- important_people -> Collapse blocks in Pessoas.
                INSERT INTO character_tab_blocks (id, character_tab_id, type, "order", title, meta, content, payload_json, created_at, updated_at)
                SELECT gen_random_uuid(), ct.id, 'Collapse',
                    ROW_NUMBER() OVER (PARTITION BY p.character_id ORDER BY p.created_at) - 1,
                    p.name,
                    (CASE p.type
                        WHEN 'Npc' THEN 'NPC' WHEN 'PlayerCharacter' THEN 'Personagem de jogador'
                        WHEN 'Faction' THEN 'Facção' WHEN 'Creature' THEN 'Criatura'
                        WHEN 'Organization' THEN 'Organização' ELSE 'Outro' END)
                        || CASE WHEN p.last_contact_at IS NOT NULL
                            THEN ' · Último contato: ' || to_char(p.last_contact_at, 'DD/MM/YYYY')
                            ELSE '' END,
                    concat_ws(E'\n\n',
                        '**Avaliação:** Confiança ' ||
                            (CASE p.trust_level WHEN 'None' THEN 'Nenhum' WHEN 'Low' THEN 'Baixo' WHEN 'Medium' THEN 'Médio' WHEN 'High' THEN 'Alto' WHEN 'Critical' THEN 'Crítico' ELSE p.trust_level END)
                            || ' · Risco ' ||
                            (CASE p.risk_level WHEN 'None' THEN 'Nenhum' WHEN 'Low' THEN 'Baixo' WHEN 'Medium' THEN 'Médio' WHEN 'High' THEN 'Alto' WHEN 'Critical' THEN 'Crítico' ELSE p.risk_level END)
                            || ' · Utilidade ' ||
                            (CASE p.utility_level WHEN 'None' THEN 'Nenhum' WHEN 'Low' THEN 'Baixo' WHEN 'Medium' THEN 'Médio' WHEN 'High' THEN 'Alto' WHEN 'Critical' THEN 'Crítico' ELSE p.utility_level END),
                        CASE WHEN p.first_impression IS NOT NULL THEN E'**Primeira impressão:**\n' || p.first_impression END,
                        CASE WHEN p.analysis IS NOT NULL THEN E'**Análise:**\n' || p.analysis END,
                        CASE WHEN p.notes IS NOT NULL THEN E'**Notas:**\n' || p.notes END),
                    NULL, p.created_at, p.updated_at
                FROM important_people p
                JOIN character_tabs ct ON ct.character_id = p.character_id AND ct.name = 'Pessoas';

                -- Pre-existing custom-tab entries -> Collapse blocks in the same tab they already lived in.
                INSERT INTO character_tab_blocks (id, character_tab_id, type, "order", title, meta, content, payload_json, created_at, updated_at)
                SELECT gen_random_uuid(), e.character_tab_id, 'Collapse',
                    ROW_NUMBER() OVER (PARTITION BY e.character_tab_id ORDER BY e.created_at) - 1,
                    e.title, NULL, e.content, NULL, e.created_at, e.updated_at
                FROM character_tab_entries e;
                """);

            // --- Now that every row has a block equivalent, drop the structured tables. ---

            migrationBuilder.DropTable(
                name: "narrative_item_tags");

            migrationBuilder.DropTable(
                name: "operation_tags");

            migrationBuilder.DropTable(
                name: "player_note_tags");

            migrationBuilder.DropTable(
                name: "theory_tags");

            migrationBuilder.DropTable(
                name: "character_attributes");

            migrationBuilder.DropTable(
                name: "character_tab_entries");

            migrationBuilder.DropTable(
                name: "important_people");

            migrationBuilder.DropTable(
                name: "narrative_items");

            migrationBuilder.DropTable(
                name: "operations");

            migrationBuilder.DropTable(
                name: "player_notes");

            migrationBuilder.DropTable(
                name: "theories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NOTE: this restores the old schema but not the old data — the Up()
            // data migration is one-way (structured rows were folded into blocks
            // and the source tables dropped). Rolling back re-creates empty tables.
            migrationBuilder.DropTable(
                name: "character_tab_blocks");

            migrationBuilder.DropColumn(
                name: "order",
                table: "character_tabs");

            migrationBuilder.CreateTable(
                name: "character_attributes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "character_tab_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_tab_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "important_people",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analysis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    first_impression = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    last_contact_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    risk_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    trust_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    utility_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_important_people", x => x.id);
                    table.ForeignKey(
                        name: "FK_important_people_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "narrative_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    importance = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    origin = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_narrative_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_narrative_items_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_narrative_items_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    objective = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    plan = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    required_resources = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    result = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    risks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_operations_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_player_notes_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_notes_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "theories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    confidence = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    evidence = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_theories", x => x.id);
                    table.ForeignKey(
                        name: "FK_theories_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "narrative_item_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    narrative_item_id = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "operation_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false)
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
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_note_id = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "theory_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theory_id = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_character_attributes_character_id",
                table: "character_attributes",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "IX_character_tab_entries_character_tab_id",
                table: "character_tab_entries",
                column: "character_tab_id");

            migrationBuilder.CreateIndex(
                name: "IX_important_people_character_id",
                table: "important_people",
                column: "character_id");

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
                name: "IX_narrative_items_character_id",
                table: "narrative_items",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "IX_narrative_items_session_id",
                table: "narrative_items",
                column: "session_id");

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
                name: "IX_operations_character_id",
                table: "operations",
                column: "character_id");

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
                name: "IX_player_notes_character_id",
                table: "player_notes",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_notes_session_id",
                table: "player_notes",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_theories_character_id",
                table: "theories",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "IX_theory_tags_tag_id",
                table: "theory_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_theory_tags_theory_id_tag_id",
                table: "theory_tags",
                columns: new[] { "theory_id", "tag_id" },
                unique: true);
        }
    }
}
