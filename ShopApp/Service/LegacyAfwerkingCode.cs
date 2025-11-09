using Microsoft.EntityFrameworkCore;
using QuadroApp.Data;
using QuadroApp.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuadroApp.Services
{
    public static class LegacyAfwerkingCode
    {
        private static char Encode(int? idx)
        {
            if (!idx.HasValue || idx.Value <= 0) return '0';
            if (idx.Value <= 9) return (char)('0' + idx.Value);
            int k = idx.Value - 10; // 0..10 => A..K
            if (k < 0 || k > 10) throw new ArgumentOutOfRangeException(nameof(idx));
            return (char)('A' + k);
        }

        private static int? Decode(char c)
        {
            if (c == '0') return null;
            if (c >= '1' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'K') return 10 + (c - 'A');
            if (c >= 'a' && c <= 'k') return 10 + (c - 'a');
            throw new ArgumentException($"Ongeldig teken: {c}");
        }

        public static string Generate(OfferteRegel o)
        {
            var code = new[]
            {
                Encode(o.Glas?.Volgnummer),
                Encode(o.PassePartout1?.Volgnummer),
                Encode(o.PassePartout2?.Volgnummer),
                Encode(o.DiepteKern?.Volgnummer),
                Encode(o.Opkleven?.Volgnummer),
                Encode(o.Rug?.Volgnummer)
            };
            return new string(code);
        }

        public static async Task ApplyAsync(AppDbContext db, OfferteRegel o, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 6)
                throw new ArgumentException("Code vereist 6 tekens (G P P D O R).");

            int?[] idx = code.Take(6).Select(Decode).ToArray();

            int gId = await db.AfwerkingsGroepen.Where(x => x.Code == 'G').Select(x => x.Id).FirstAsync();
            int pId = await db.AfwerkingsGroepen.Where(x => x.Code == 'P').Select(x => x.Id).FirstAsync();
            int dId = await db.AfwerkingsGroepen.Where(x => x.Code == 'D').Select(x => x.Id).FirstAsync();
            int oId = await db.AfwerkingsGroepen.Where(x => x.Code == 'O').Select(x => x.Id).FirstAsync();
            int rId = await db.AfwerkingsGroepen.Where(x => x.Code == 'R').Select(x => x.Id).FirstAsync();

            async Task<AfwerkingsOptie?> Find(int groepId, int? volg)
            {
                if (!volg.HasValue) return null;
                return await db.AfwerkingsOpties.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AfwerkingsGroepId == groepId && a.Volgnummer == volg.Value);
            }

            o.Glas = await Find(gId, idx[0]);
            o.PassePartout1 = await Find(pId, idx[1]);
            o.PassePartout2 = await Find(pId, idx[2]);
            o.DiepteKern = await Find(dId, idx[3]);
            o.Opkleven = await Find(oId, idx[4]);
            o.Rug = await Find(rId, idx[5]);
        }
    }
}
