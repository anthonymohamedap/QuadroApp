using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuadroApp.Migrations
{
    /// <inheritdoc />
    public partial class OfferteRegel_NoAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AfwerkingsGroepen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfwerkingsGroepen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instellingen",
                columns: table => new
                {
                    Sleutel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Waarde = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instellingen", x => x.Sleutel);
                });

            migrationBuilder.CreateTable(
                name: "Klanten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Voornaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Achternaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefoon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Straat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nummer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gemeente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opmerking = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Klanten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leveranciers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leveranciers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Offertes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlantId = table.Column<int>(type: "int", nullable: true),
                    SubtotaalExBtw = table.Column<decimal>(type: "decimal(18,2)", precision: 10, scale: 2, nullable: false),
                    BtwBedrag = table.Column<decimal>(type: "decimal(18,2)", precision: 10, scale: 2, nullable: false),
                    TotaalInclBtw = table.Column<decimal>(type: "decimal(18,2)", precision: 10, scale: 2, nullable: false),
                    Opmerking = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offertes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offertes_Klanten_KlantId",
                        column: x => x.KlantId,
                        principalTable: "Klanten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AfwerkingsOpties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AfwerkingsGroepId = table.Column<int>(type: "int", nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Volgnummer = table.Column<int>(type: "int", nullable: false),
                    KostprijsPerM2 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    WinstMarge = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    AfvalPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    VasteKost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    WerkMinuten = table.Column<int>(type: "int", nullable: false),
                    LeverancierId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfwerkingsOpties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AfwerkingsOpties_AfwerkingsGroepen_AfwerkingsGroepId",
                        column: x => x.AfwerkingsGroepId,
                        principalTable: "AfwerkingsGroepen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AfwerkingsOpties_Leveranciers_LeverancierId",
                        column: x => x.LeverancierId,
                        principalTable: "Leveranciers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TypeLijsten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Artikelnummer = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LeverancierCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LeverancierId = table.Column<int>(type: "int", nullable: false),
                    BreedteCm = table.Column<int>(type: "int", nullable: false),
                    Soort = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDealer = table.Column<bool>(type: "bit", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrijsPerMeter = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    WinstMargeFactor = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    AfvalPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    VasteKost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    WerkMinuten = table.Column<int>(type: "int", nullable: false),
                    VoorraadMeter = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    InventarisKost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    LaatsteUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MinimumVoorraad = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeLijsten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TypeLijsten_Leveranciers_LeverancierId",
                        column: x => x.LeverancierId,
                        principalTable: "Leveranciers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WerkBonnen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferteId = table.Column<int>(type: "int", nullable: false),
                    AfhaalDatum = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotaalPrijsIncl = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WerkBonnen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WerkBonnen_Offertes_OfferteId",
                        column: x => x.OfferteId,
                        principalTable: "Offertes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lijsten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeLijstId = table.Column<int>(type: "int", nullable: false),
                    LengteMeter = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lijsten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lijsten_TypeLijsten_TypeLijstId",
                        column: x => x.TypeLijstId,
                        principalTable: "TypeLijsten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfferteRegels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferteId = table.Column<int>(type: "int", nullable: false),
                    AantalStuks = table.Column<int>(type: "int", nullable: false),
                    BreedteCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HoogteCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TypeLijstId = table.Column<int>(type: "int", nullable: true),
                    GlasId = table.Column<int>(type: "int", nullable: true),
                    PassePartout1Id = table.Column<int>(type: "int", nullable: true),
                    PassePartout2Id = table.Column<int>(type: "int", nullable: true),
                    DiepteKernId = table.Column<int>(type: "int", nullable: true),
                    OpklevenId = table.Column<int>(type: "int", nullable: true),
                    RugId = table.Column<int>(type: "int", nullable: true),
                    ExtraWerkMinuten = table.Column<int>(type: "int", nullable: false),
                    ExtraPrijs = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Korting = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LegacyCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    SubtotaalExBtw = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BtwBedrag = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotaalInclBtw = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferteRegels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferteRegels_AfwerkingsOpties_DiepteKernId",
                        column: x => x.DiepteKernId,
                        principalTable: "AfwerkingsOpties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OfferteRegels_AfwerkingsOpties_GlasId",
                        column: x => x.GlasId,
                        principalTable: "AfwerkingsOpties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OfferteRegels_AfwerkingsOpties_OpklevenId",
                        column: x => x.OpklevenId,
                        principalTable: "AfwerkingsOpties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OfferteRegels_AfwerkingsOpties_PassePartout1Id",
                        column: x => x.PassePartout1Id,
                        principalTable: "AfwerkingsOpties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OfferteRegels_AfwerkingsOpties_PassePartout2Id",
                        column: x => x.PassePartout2Id,
                        principalTable: "AfwerkingsOpties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OfferteRegels_AfwerkingsOpties_RugId",
                        column: x => x.RugId,
                        principalTable: "AfwerkingsOpties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OfferteRegels_Offertes_OfferteId",
                        column: x => x.OfferteId,
                        principalTable: "Offertes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferteRegels_TypeLijsten_TypeLijstId",
                        column: x => x.TypeLijstId,
                        principalTable: "TypeLijsten",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WerkTaken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WerkBonId = table.Column<int>(type: "int", nullable: false),
                    GeplandVan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeplandTot = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuurMinuten = table.Column<int>(type: "int", nullable: false),
                    Omschrijving = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WerkTaken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WerkTaken_WerkBonnen_WerkBonId",
                        column: x => x.WerkBonId,
                        principalTable: "WerkBonnen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AfwerkingsOpties_AfwerkingsGroepId_Volgnummer",
                table: "AfwerkingsOpties",
                columns: new[] { "AfwerkingsGroepId", "Volgnummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AfwerkingsOpties_LeverancierId",
                table: "AfwerkingsOpties",
                column: "LeverancierId");

            migrationBuilder.CreateIndex(
                name: "IX_Lijsten_TypeLijstId",
                table: "Lijsten",
                column: "TypeLijstId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferteRegels_DiepteKernId",
                table: "OfferteRegels",
                column: "DiepteKernId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferteRegels_GlasId",
                table: "OfferteRegels",
                column: "GlasId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferteRegels_OfferteId",
                table: "OfferteRegels",
                column: "OfferteId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferteRegels_OpklevenId",
                table: "OfferteRegels",
                column: "OpklevenId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferteRegels_PassePartout1Id",
                table: "OfferteRegels",
                column: "PassePartout1Id");

            migrationBuilder.CreateIndex(
                name: "IX_OfferteRegels_PassePartout2Id",
                table: "OfferteRegels",
                column: "PassePartout2Id");

            migrationBuilder.CreateIndex(
                name: "IX_OfferteRegels_RugId",
                table: "OfferteRegels",
                column: "RugId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferteRegels_TypeLijstId",
                table: "OfferteRegels",
                column: "TypeLijstId");

            migrationBuilder.CreateIndex(
                name: "IX_Offertes_KlantId",
                table: "Offertes",
                column: "KlantId");

            migrationBuilder.CreateIndex(
                name: "IX_TypeLijsten_LeverancierId",
                table: "TypeLijsten",
                column: "LeverancierId");

            migrationBuilder.CreateIndex(
                name: "IX_WerkBonnen_OfferteId",
                table: "WerkBonnen",
                column: "OfferteId");

            migrationBuilder.CreateIndex(
                name: "IX_WerkTaken_WerkBonId",
                table: "WerkTaken",
                column: "WerkBonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Instellingen");

            migrationBuilder.DropTable(
                name: "Lijsten");

            migrationBuilder.DropTable(
                name: "OfferteRegels");

            migrationBuilder.DropTable(
                name: "WerkTaken");

            migrationBuilder.DropTable(
                name: "AfwerkingsOpties");

            migrationBuilder.DropTable(
                name: "TypeLijsten");

            migrationBuilder.DropTable(
                name: "WerkBonnen");

            migrationBuilder.DropTable(
                name: "AfwerkingsGroepen");

            migrationBuilder.DropTable(
                name: "Leveranciers");

            migrationBuilder.DropTable(
                name: "Offertes");

            migrationBuilder.DropTable(
                name: "Klanten");
        }
    }
}
