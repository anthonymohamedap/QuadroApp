using Microsoft.EntityFrameworkCore;
using QuadroApp.Data;
using QuadroApp.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuadroApp.Services
{
    public class PricingService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public PricingService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

        public async Task BerekenAsync(int offerteId)
        {
            using var db = _factory.CreateDbContext();

            var inst = await db.Instellingen.ToDictionaryAsync(x => x.Sleutel, x => x.Waarde);
            decimal uurloon = decimal.TryParse(inst.GetValueOrDefault("Uurloon"), out var u) ? u : 45m;
            decimal btwPct = decimal.TryParse(inst.GetValueOrDefault("BtwPercent"), out var b) ? b : 21m;

            var o = await db.Offertes
                .Include(x => x.Regels)
                    .ThenInclude(r => r.TypeLijst)
                .Include(x => x.Regels).ThenInclude(r => r.Glas)
                .Include(x => x.Regels).ThenInclude(r => r.PassePartout1)
                .Include(x => x.Regels).ThenInclude(r => r.PassePartout2)
                .Include(x => x.Regels).ThenInclude(r => r.DiepteKern)
                .Include(x => x.Regels).ThenInclude(r => r.Opkleven)
                .Include(x => x.Regels).ThenInclude(r => r.Rug)
                .FirstAsync(x => x.Id == offerteId);

            decimal totaalEx = 0, totaalBtw = 0, totaalIncl = 0;

            foreach (var r in o.Regels)
            {
                // — lijst + afwerkingen — (zelfde formules als je had, maar nu per regel)
                decimal lineEx = 0;

                // Lijst (als TypeLijst is gekozen)
                if (r.TypeLijst != null)
                {
                    var perimMm = (r.BreedteCm + r.HoogteCm) * 2 * 10m + (r.TypeLijst.BreedteCm * 10m);
                    var lengteM = perimMm / 1000m;
                    var kost = r.TypeLijst.PrijsPerMeter * lengteM;
                    var afval = kost * (r.TypeLijst.AfvalPercentage / 100m);
                    var arbeid = (r.TypeLijst.WerkMinuten / 60m) * uurloon;
                    var excl = (kost + afval) * (1 + r.TypeLijst.WinstMargeFactor) + r.TypeLijst.VasteKost + arbeid;
                    lineEx += Math.Round(excl, 2);
                }

                // Helper voor opties (G/P/P/D/O/R)
                decimal CalcOpt(AfwerkingsOptie? o1)
                {
                    if (o1 == null) return 0m;
                    var m2 = (r.BreedteCm * r.HoogteCm) / 10_000m;
                    var kost = o1.KostprijsPerM2 * m2 + o1.VasteKost;
                    var afval = kost * (o1.AfvalPercentage / 100m);
                    var arbeid = (o1.WerkMinuten / 60m) * uurloon;
                    return Math.Round((kost + afval) * (1 + o1.WinstMarge) + arbeid, 2);
                }

                lineEx += CalcOpt(r.Glas);
                lineEx += CalcOpt(r.PassePartout1);
                lineEx += CalcOpt(r.PassePartout2);
                lineEx += CalcOpt(r.DiepteKern);
                lineEx += CalcOpt(r.Opkleven);
                lineEx += CalcOpt(r.Rug);

                // Extra's, korting en aantal
                lineEx += (r.ExtraWerkMinuten / 60m) * uurloon + r.ExtraPrijs;
                lineEx -= r.Korting;
                lineEx = lineEx * r.AantalStuks;

                var lineBtw = Math.Round(lineEx * (btwPct / 100m), 2);
                var lineIncl = lineEx + lineBtw;

                r.SubtotaalExBtw = lineEx;
                r.BtwBedrag = lineBtw;
                r.TotaalInclBtw = lineIncl;

                totaalEx += lineEx;
                totaalBtw += lineBtw;
                totaalIncl += lineIncl;
            }

            o.SubtotaalExBtw = totaalEx;
            o.BtwBedrag = totaalBtw;
            o.TotaalInclBtw = totaalIncl;

            await db.SaveChangesAsync();
        }
    }
}
