using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuadroApp.Model
{
    public class OfferteRegel
    {
        public int Id { get; set; }

        // FK naar Offerte (parent)
        public int OfferteId { get; set; }
        public Offerte? Offerte { get; set; }

        // Invoer
        [Range(1, 9999)]
        public int AantalStuks { get; set; } = 1;

        public decimal BreedteCm { get; set; }
        public decimal HoogteCm { get; set; }

        // Lijsttype
        public int? TypeLijstId { get; set; }
        public TypeLijst? TypeLijst { get; set; }

        // Afwerkingen (per groep één optie)
        public int? GlasId { get; set; }
        public AfwerkingsOptie? Glas { get; set; }

        public int? PassePartout1Id { get; set; }
        public AfwerkingsOptie? PassePartout1 { get; set; }

        public int? PassePartout2Id { get; set; }
        public AfwerkingsOptie? PassePartout2 { get; set; }

        public int? DiepteKernId { get; set; }
        public AfwerkingsOptie? DiepteKern { get; set; }

        public int? OpklevenId { get; set; }
        public AfwerkingsOptie? Opkleven { get; set; }

        public int? RugId { get; set; }
        public AfwerkingsOptie? Rug { get; set; }

        // Work / extra
        public int ExtraWerkMinuten { get; set; } = 0;
        public decimal ExtraPrijs { get; set; } = 0m;
        public decimal Korting { get; set; } = 0m;

        // Legacy-code per regel (GPPDOR), optioneel
        [MaxLength(6)]
        public string? LegacyCode { get; set; }

        // Resultaat (berekend en opgeslagen)
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotaalExBtw { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BtwBedrag { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotaalInclBtw { get; set; }
    }
}
