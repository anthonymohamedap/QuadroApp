using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuadroApp.Model
{
    public class Offerte
    {
        public int Id { get; set; }

        // Nieuw: klant-koppeling
        public int? KlantId { get; set; }
        public Klant? Klant { get; set; }

        // Verzameling regels
        public ICollection<OfferteRegel> Regels { get; set; } = new List<OfferteRegel>();

        // Offerte-totalen
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotaalExBtw { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BtwBedrag { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotaalInclBtw { get; set; }

        // Optioneel: opmerking, status, geldigheidsdatum, …
        public string? Opmerking { get; set; }
    }
}
