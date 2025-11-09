using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuadroApp.Migrations
{
    /// <inheritdoc />
    public partial class PlanningModelUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Resource",
                table: "WerkTaken",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WerkTaken",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AangemaaktOp",
                table: "WerkBonnen",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BijgewerktOp",
                table: "WerkBonnen",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WerkBonnen",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "WerkBonnen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WerkTaken_GeplandVan",
                table: "WerkTaken",
                column: "GeplandVan");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WerkTaak_Duur_Positive",
                table: "WerkTaken",
                sql: "[DuurMinuten] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WerkTaak_Tot_After_Van",
                table: "WerkTaken",
                sql: "[GeplandTot] > [GeplandVan]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WerkTaken_GeplandVan",
                table: "WerkTaken");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WerkTaak_Duur_Positive",
                table: "WerkTaken");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WerkTaak_Tot_After_Van",
                table: "WerkTaken");

            migrationBuilder.DropColumn(
                name: "Resource",
                table: "WerkTaken");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WerkTaken");

            migrationBuilder.DropColumn(
                name: "AangemaaktOp",
                table: "WerkBonnen");

            migrationBuilder.DropColumn(
                name: "BijgewerktOp",
                table: "WerkBonnen");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WerkBonnen");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WerkBonnen");
        }
    }
}
