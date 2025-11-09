using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuadroApp.Model
{
    public class Lijst
    {
        public int Id { get; set; }

        [Required]
        public int TypeLijstId { get; set; }
        public TypeLijst TypeLijst { get; set; } = null!;

        [Precision(10, 2)]
        public double LengteMeter { get; set; }

        [NotMapped]
        public decimal Prijs => TypeLijst.PrijsPerMeter * (decimal)LengteMeter;
    }
}
