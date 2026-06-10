using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerdeProje.Migrations
{
    public partial class AddAdresAndKartTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. ADRESLER TABLOSU
            migrationBuilder.CreateTable(
                name: "Adresler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcikAdres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adresler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adresler_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 2. KARTLAR TABLOSU
            migrationBuilder.CreateTable(
                name: "Kartlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KartBasligi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KartNumarasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kartlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kartlar_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adresler_UserId",
                table: "Adresler",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Kartlar_UserId",
                table: "Kartlar",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adresler");

            migrationBuilder.DropTable(
                name: "Kartlar");
        }
    }
}