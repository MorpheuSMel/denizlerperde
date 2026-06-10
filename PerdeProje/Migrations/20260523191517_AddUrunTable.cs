using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerdeProje.Migrations
{
    /// <inheritdoc />
    public partial class AddUrunTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adresler_Users_UserId",
                table: "Adresler");

            migrationBuilder.DropForeignKey(
                name: "FK_Kartlar_Users_UserId",
                table: "Kartlar");

            migrationBuilder.DropIndex(
                name: "IX_Kartlar_UserId",
                table: "Kartlar");

            migrationBuilder.DropIndex(
                name: "IX_Adresler_UserId",
                table: "Adresler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Kartlar_UserId",
                table: "Kartlar",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Adresler_UserId",
                table: "Adresler",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adresler_Users_UserId",
                table: "Adresler",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Kartlar_Users_UserId",
                table: "Kartlar",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
