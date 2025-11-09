using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuadroApp.Migrations
{
    /// <inheritdoc />
    public partial class KLantUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Beschrijving",
                table: "TypeLijsten",
                newName: "Opmerking");

            migrationBuilder.AddColumn<string>(
                name: "BtwNummer",
                table: "Klanten",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BtwNummer",
                table: "Klanten");

            migrationBuilder.RenameColumn(
                name: "Opmerking",
                table: "TypeLijsten",
                newName: "Beschrijving");
        }
    }
}
