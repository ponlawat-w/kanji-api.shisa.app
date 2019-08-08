using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DatabaseContext.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kanjis",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    character = table.Column<string>(type: "VARCHAR(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_kanjis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parts",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    kanji = table.Column<long>(nullable: false),
                    part = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_parts", x => x.id);
                    table.ForeignKey(
                        name: "fkey_parts_kanji",
                        column: x => x.kanji,
                        principalTable: "kanjis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fkey_parts_part",
                        column: x => x.part,
                        principalTable: "kanjis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "structures",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    kanji = table.Column<long>(nullable: false),
                    structure = table.Column<string>(type: "VARCHAR(200)", nullable: false),
                    note = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_structures", x => x.id);
                    table.ForeignKey(
                        name: "fkey_structures_kanji",
                        column: x => x.kanji,
                        principalTable: "kanjis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_kanjis_character",
                table: "kanjis",
                column: "character",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parts_part",
                table: "parts",
                column: "part");

            migrationBuilder.CreateIndex(
                name: "idx_parts_kanji_part",
                table: "parts",
                columns: new[] { "kanji", "part" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_structures_kanji_structure",
                table: "structures",
                columns: new[] { "kanji", "structure" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parts");

            migrationBuilder.DropTable(
                name: "structures");

            migrationBuilder.DropTable(
                name: "kanjis");
        }
    }
}
