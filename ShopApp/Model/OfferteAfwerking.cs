using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadroApp.Model
{
    public class OfferteAfwerking
    {
        public int OfferteId { get; set; }
        public Offerte Offerte { get; set; } = null!;
        public AfwerkingsGroep Groep { get; set; } = null!;

        public int OptieId { get; set; }
        public AfwerkingsOptie Optie { get; set; } = null!;
    }

}
