using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerdeProje.Migrations
{
    /// <inheritdoc />
    public partial class FixKartTableColumnsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cvv",
                table: "Kartlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KartUzerindekiIsim",
                table: "Kartlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SonKullanmaTarihi",
                table: "Kartlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cvv",
                table: "Kartlar");

            migrationBuilder.DropColumn(
                name: "KartUzerindekiIsim",
                table: "Kartlar");

            migrationBuilder.DropColumn(
                name: "SonKullanmaTarihi",
                table: "Kartlar");
        }
    }
}
