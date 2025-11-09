using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuadroApp.Model
{
    [Index(nameof(GeplandVan))]
    [Index(nameof(WerkBonId))]
    public class WerkTaak
    {
        public int Id { get; set; }
        public int WerkBonId { get; set; }
        public WerkBon WerkBon { get; set; } = null!;

        // Tip: hou het bij lokale tijd in je app; als je ooit timezones nodig hebt, migreer naar DateTimeOffset.
        public DateTime GeplandVan { get; set; }   // start (local)
        public DateTime GeplandTot { get; set; }   // einde (local)

        public int DuurMinuten { get; set; }       // bewaak met check-constraint

        [MaxLength(200)]
        public string Omschrijving { get; set; } = string.Empty;

        // Optioneel: wie voert het uit?
        [MaxLength(80)]
        public string? Resource { get; set; }

        [Timestamp] public byte[]? RowVersion { get; set; }
    }
}
