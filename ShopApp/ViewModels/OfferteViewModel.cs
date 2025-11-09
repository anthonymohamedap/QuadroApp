using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuadroApp.Data;
using QuadroApp.Model;
using QuadroApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuadroApp.ViewModels
{
    public partial class OfferteViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly PricingService _pricing;

        // ───────── Debug helpers ─────────
#if DEBUG
        private readonly string _traceId = Guid.NewGuid().ToString("N")[..8];
        private int _seq = 0;
        private void Log(string msg) =>
            Console.WriteLine($"[{DateTime.Now:O}] [OfferteVM:{_traceId}] #{++_seq} {msg}");
        private T LogReturn<T>(string heading, T value)
        {
            Log($"{heading}: {value}");
            return value;
        }
#else
        private void Log(string msg) { }
        private T LogReturn<T>(string heading, T value) => value;
#endif

        // ───────── Databronnen voor de UI ─────────
        [ObservableProperty] private ObservableCollection<TypeLijst> typeLijsten = new();
        [ObservableProperty] private ObservableCollection<AfwerkingsOptie> glasOpties = new();
        [ObservableProperty] private ObservableCollection<AfwerkingsOptie> passe1Opties = new();
        [ObservableProperty] private ObservableCollection<AfwerkingsOptie> passe2Opties = new();
        [ObservableProperty] private ObservableCollection<AfwerkingsOptie> diepteOpties = new();
        [ObservableProperty] private ObservableCollection<AfwerkingsOptie> opkleefOpties = new();
        [ObservableProperty] private ObservableCollection<AfwerkingsOptie> rugOpties = new();

        // ───────── Offerte/Regels/Klant ─────────
        [ObservableProperty] private Offerte? offerte;
        [ObservableProperty] private ObservableCollection<Klant> klanten = new();
        [ObservableProperty] private Klant? selectedKlant;

        [ObservableProperty] private ObservableCollection<OfferteRegel> regels = new();
        [ObservableProperty] private OfferteRegel? selectedRegel;

        // ───────── Totals: komen uit de Offerte na BEREKEN ─────────
        public decimal OfferteEx => Offerte?.SubtotaalExBtw ?? 0m;
        public decimal OfferteBtw => Offerte?.BtwBedrag ?? 0m;
        public decimal OfferteIncl => Offerte?.TotaalInclBtw ?? 0m;
        private void RefreshOfferteTotals()
        {
            Log($"RefreshOfferteTotals Ex={OfferteEx}, Btw={OfferteBtw}, Incl={OfferteIncl}");
            OnPropertyChanged(nameof(OfferteEx));
            OnPropertyChanged(nameof(OfferteBtw));
            OnPropertyChanged(nameof(OfferteIncl));
        }

        // ───────── Planning (blijft voor kalender) ─────────
        [ObservableProperty] private DateTimeOffset? planDatum = DateTimeOffset.Now.Date;
        [ObservableProperty] private TimeSpan planTijd = new TimeSpan(9, 0, 0);
        [ObservableProperty] private int planDuurMinuten = 60;

        // ───────── UI state ─────────
        [ObservableProperty] private string? foutmelding;
        partial void OnFoutmeldingChanged(string? oldValue, string? newValue)
        {
            if (!string.IsNullOrWhiteSpace(newValue))
                Log($"Foutmelding set: {newValue}");
        }

        [ObservableProperty] private bool isBusy;
        partial void OnIsBusyChanged(bool oldValue, bool newValue) => Log($"IsBusy: {oldValue} -> {newValue}");

        public event Action<string>? NavigatieGevraagd;

        // ───────── ctor / init ─────────
        public OfferteViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            _pricing = new PricingService(factory);
            Log("CTOR start");
            _ = InitAsync();
            Log("CTOR end (InitAsync fire-and-forget)");
        }

        private async Task InitAsync()
        {
            Log("InitAsync: START");
            var sw = Stopwatch.StartNew();
            try
            {
                using var db = _factory.CreateDbContext();
                Log("InitAsync: DB context created");

                var typeLijstenList = await db.TypeLijsten.AsNoTracking().OrderBy(t => t.Artikelnummer).ToListAsync();
                Log($"InitAsync: Loaded TypeLijsten count={typeLijstenList.Count}");
                TypeLijsten = new ObservableCollection<TypeLijst>(typeLijstenList);

                var klantenList = await db.Klanten.AsNoTracking()
                    .OrderBy(k => k.Achternaam).ThenBy(k => k.Voornaam).ToListAsync();
                Log($"InitAsync: Loaded Klanten count={klantenList.Count}");
                Klanten = new ObservableCollection<Klant>(klantenList);

                await LoadAfwerkingOptiesAsync();

                Offerte = new Offerte();
                var firstType = TypeLijsten.FirstOrDefault();
                Log($"InitAsync: First TypeLijst = {(firstType == null ? "null" : $"{firstType.Id}:{firstType.Artikelnummer}")}");

                var r = new OfferteRegel
                {
                    AantalStuks = 1,
                    BreedteCm = 30,
                    HoogteCm = 40,
                    TypeLijst = firstType
                };
                if (r.TypeLijst != null) r.TypeLijstId = r.TypeLijst.Id;

                Offerte.Regels.Add(r);
                Regels = new ObservableCollection<OfferteRegel>(Offerte.Regels);
                SelectedRegel = r;
                Log($"InitAsync: Initial regel added (BxH={r.BreedteCm}x{r.HoogteCm}, TypeLijstId={r.TypeLijstId})");
                RefreshOfferteTotals();
            }
            catch (Exception ex)
            {
                Log($"InitAsync: ERROR {ex}");
                Foutmelding = ex.Message;
            }
            finally
            {
                sw.Stop();
                Log($"InitAsync: END in {sw.ElapsedMilliseconds} ms");
            }
        }

        private async Task LoadAfwerkingOptiesAsync()
        {
            Log("LoadAfwerkingOptiesAsync: START");
            var sw = Stopwatch.StartNew();
            try
            {
                GlasOpties = await OptiesVoorGroepAsync('G'); Log($"GlasOpties: {GlasOpties.Count}");
                Passe1Opties = await OptiesVoorGroepAsync('P'); Log($"Passe1Opties: {Passe1Opties.Count}");
                Passe2Opties = await OptiesVoorGroepAsync('P'); Log($"Passe2Opties: {Passe2Opties.Count}");
                DiepteOpties = await OptiesVoorGroepAsync('D'); Log($"DiepteOpties: {DiepteOpties.Count}");
                OpkleefOpties = await OptiesVoorGroepAsync('O'); Log($"OpkleefOpties: {OpkleefOpties.Count}");
                RugOpties = await OptiesVoorGroepAsync('R'); Log($"RugOpties: {RugOpties.Count}");
            }
            catch (Exception ex)
            {
                Log($"LoadAfwerkingOptiesAsync: ERROR {ex}");
                Foutmelding = ex.Message;
            }
            finally
            {
                sw.Stop();
                Log($"LoadAfwerkingOptiesAsync: END in {sw.ElapsedMilliseconds} ms");
            }
        }

        private async Task<ObservableCollection<AfwerkingsOptie>> OptiesVoorGroepAsync(char code)
        {
            Log($"OptiesVoorGroepAsync('{code}'): START");
            using var db = _factory.CreateDbContext();
            try
            {
                var groepId = await db.AfwerkingsGroepen
                                      .Where(g => g.Code == code)
                                      .Select(g => g.Id)
                                      .FirstAsync();
                Log($"OptiesVoorGroepAsync('{code}'): groepId={groepId}");

                var list = await db.AfwerkingsOpties.AsNoTracking()
                                .Where(a => a.AfwerkingsGroepId == groepId)
                                .OrderBy(a => a.Volgnummer).ThenBy(a => a.Naam)
                                .ToListAsync();
                Log($"OptiesVoorGroepAsync('{code}'): loaded {list.Count} opties");
                return new ObservableCollection<AfwerkingsOptie>(list);
            }
            catch (Exception ex)
            {
                Log($"OptiesVoorGroepAsync('{code}'): ERROR {ex}");
                throw;
            }
            finally
            {
                Log($"OptiesVoorGroepAsync('{code}'): END");
            }
        }

        // ───────── Identity-INSERT bescherming ─────────
        private void PrepareOfferteForSave(AppDbContext db)
        {
            Log("PrepareOfferteForSave: START");
            if (Offerte is null)
            {
                Log("PrepareOfferteForSave: Offerte is null, skip");
                return;
            }

            Offerte.KlantId = SelectedKlant?.Id;
            Log($"PrepareOfferteForSave: KlantId set to {Offerte.KlantId?.ToString() ?? "null"}");

            var cleaned = new List<OfferteRegel>();
            int i = 0;

            foreach (var s in Regels)
            {
                i++;
                Log($"PrepareOfferteForSave: Copy regel #{i} (Id={s.Id}, TL={s.TypeLijstId}, G={s.GlasId}, P1={s.PassePartout1Id}, P2={s.PassePartout2Id}, D={s.DiepteKernId}, O={s.OpklevenId}, R={s.RugId})");

                var r = new OfferteRegel
                {
                    Id = s.Id,
                    AantalStuks = s.AantalStuks,
                    BreedteCm = s.BreedteCm,
                    HoogteCm = s.HoogteCm,

                    TypeLijstId = s.TypeLijst?.Id ?? s.TypeLijstId,
                    GlasId = s.Glas?.Id ?? s.GlasId,
                    PassePartout1Id = s.PassePartout1?.Id ?? s.PassePartout1Id,
                    PassePartout2Id = s.PassePartout2?.Id ?? s.PassePartout2Id,
                    DiepteKernId = s.DiepteKern?.Id ?? s.DiepteKernId,
                    OpklevenId = s.Opkleven?.Id ?? s.OpklevenId,
                    RugId = s.Rug?.Id ?? s.RugId,

                    ExtraWerkMinuten = s.ExtraWerkMinuten,
                    ExtraPrijs = s.ExtraPrijs,
                    Korting = s.Korting,
                    LegacyCode = s.LegacyCode
                };

                r.TypeLijst = null;
                r.Glas = null;
                r.PassePartout1 = null;
                r.PassePartout2 = null;
                r.DiepteKern = null;
                r.Opkleven = null;
                r.Rug = null;

                cleaned.Add(r);
            }

            Offerte.Regels.Clear();
            foreach (var r in cleaned)
                Offerte.Regels.Add(r);

            var affected = 0;
            foreach (var e in db.ChangeTracker.Entries()
                     .Where(e => e.Entity is TypeLijst || e.Entity is AfwerkingsOptie))
            {
                e.State = EntityState.Unchanged;
                affected++;
            }
            Log($"PrepareOfferteForSave: Marked {affected} reference entries as Unchanged");
            Log("PrepareOfferteForSave: END");
        }

        // ───────── Basis regelbeheer (optioneel) ─────────
        [RelayCommand]
        private void RegelToevoegen()
        {
            Log("RegelToevoegen: START");
            if (Offerte is null) { Log("RegelToevoegen: Offerte is null"); return; }
            var r = new OfferteRegel { AantalStuks = 1, BreedteCm = 30, HoogteCm = 40 };
            Offerte.Regels.Add(r);
            Regels.Add(r);
            SelectedRegel = r;
            Log($"RegelToevoegen: New regel added (Id={r.Id}, BxH={r.BreedteCm}x{r.HoogteCm})");
        }

        [RelayCommand]
        private void RegelDupliceren()
        {
            Log("RegelDupliceren: START");
            if (Offerte is null || SelectedRegel is null)
            {
                Log("RegelDupliceren: Offerte or SelectedRegel is null");
                return;
            }
            var s = SelectedRegel;
            var r = new OfferteRegel
            {
                AantalStuks = s.AantalStuks,
                BreedteCm = s.BreedteCm,
                HoogteCm = s.HoogteCm,
                TypeLijstId = s.TypeLijstId,
                GlasId = s.GlasId,
                PassePartout1Id = s.PassePartout1Id,
                PassePartout2Id = s.PassePartout2Id,
                DiepteKernId = s.DiepteKernId,
                OpklevenId = s.OpklevenId,
                RugId = s.RugId,
                ExtraWerkMinuten = s.ExtraWerkMinuten,
                ExtraPrijs = s.ExtraPrijs,
                Korting = s.Korting,
                LegacyCode = s.LegacyCode
            };
            Offerte.Regels.Add(r);
            Regels.Add(r);
            SelectedRegel = r;
            Log($"RegelDupliceren: Duplicated regel -> New(Id={r.Id}, TL={r.TypeLijstId}, G={r.GlasId}, P1={r.PassePartout1Id}, P2={r.PassePartout2Id}, D={r.DiepteKernId}, O={r.OpklevenId}, R={r.RugId})");
        }

        [RelayCommand]
        private void RegelVerwijderen()
        {
            Log("RegelVerwijderen: START");
            if (Offerte is null || SelectedRegel is null)
            {
                Log("RegelVerwijderen: Offerte or SelectedRegel is null");
                return;
            }
            Log($"RegelVerwijderen: Removing Id={SelectedRegel.Id}");
            Offerte.Regels.Remove(SelectedRegel);
            Regels.Remove(SelectedRegel);
            SelectedRegel = Regels.FirstOrDefault();
            Log($"RegelVerwijderen: New SelectedRegel Id={(SelectedRegel?.Id.ToString() ?? "null")}");
        }

        [RelayCommand]
        private async Task NieuweKlantAsync()
        {
            Log("NieuweKlantAsync: START");
            using var db = _factory.CreateDbContext();
            var k = new Klant { Voornaam = "Nieuwe", Achternaam = "Klant" };
            db.Klanten.Add(k);
            await db.SaveChangesAsync();
            Log($"NieuweKlantAsync: Created Klant Id={k.Id}");
            Klanten.Add(k);
            SelectedKlant = k;
            Log("NieuweKlantAsync: END");
        }

        // ───────── BEREKEN: enkel op klik ─────────
        [RelayCommand]
        private async Task BerekenAsync()
        {
            Log("BerekenAsync: START");
            var sw = Stopwatch.StartNew();
            if (Offerte is null) { Log("BerekenAsync: Offerte null -> return"); return; }

            try
            {
                IsBusy = true;
                using var db = _factory.CreateDbContext();
                Log($"BerekenAsync: Context created. Regels UI count={Regels.Count}");

                Offerte.Regels.Clear();
                foreach (var r in Regels) Offerte.Regels.Add(r);
                Log($"BerekenAsync: Offerte.Regels synced. Count={Offerte.Regels.Count}");

                PrepareOfferteForSave(db);

                if (Offerte.Id == 0)
                {
                    db.Offertes.Add(Offerte);
                    Log("BerekenAsync: Offerte added (new).");
                }
                else
                {
                    db.Offertes.Update(Offerte);
                    Log($"BerekenAsync: Offerte updated (Id={Offerte.Id}).");
                }

                var saved = await db.SaveChangesAsync();
                Log($"BerekenAsync: SaveChanges -> {saved} entries affected. Offerte.Id={Offerte.Id}");

                Log("BerekenAsync: Pricing START");
                var swPrice = Stopwatch.StartNew();
                await _pricing.BerekenAsync(Offerte.Id);
                swPrice.Stop();
                Log($"BerekenAsync: Pricing END in {swPrice.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                Log($"BerekenAsync: ERROR {ex}");
                Foutmelding = ex.InnerException?.Message ?? ex.Message;
                return; // voorkom verdere UI updates bij error
            }
            finally
            {
                IsBusy = false;
                sw.Stop();
                Log($"BerekenAsync: END in {sw.ElapsedMilliseconds} ms");
            }

            // 🔄 BELANGRIJK: verse context voor reload
            using (var db2 = _factory.CreateDbContext())
            {
                Log("BerekenAsync: Reload START (with includes, fresh context)");
                var loaded = await db2.Offertes
                    .Include(o => o.Regels).ThenInclude(r => r.TypeLijst)
                    .Include(o => o.Regels).ThenInclude(r => r.Glas)
                    .Include(o => o.Regels).ThenInclude(r => r.PassePartout1)
                    .Include(o => o.Regels).ThenInclude(r => r.PassePartout2)
                    .Include(o => o.Regels).ThenInclude(r => r.DiepteKern)
                    .Include(o => o.Regels).ThenInclude(r => r.Opkleven)
                    .Include(o => o.Regels).ThenInclude(r => r.Rug)
                    .FirstAsync(x => x.Id == Offerte!.Id);

                Offerte = loaded;
                Regels = new ObservableCollection<OfferteRegel>(loaded.Regels);
                SelectedRegel = Regels.FirstOrDefault();
                Log($"BerekenAsync: Reloaded. Regels={Regels.Count}, SelectedRegel.Id={(SelectedRegel?.Id.ToString() ?? "null")}, Totals=Ex:{OfferteEx} Btw:{OfferteBtw} Incl:{OfferteIncl}");
                RefreshOfferteTotals();
            }
        }

        // ───────── OPSLAAN: zonder berekening ─────────
        [RelayCommand]
        private async Task SaveAsync()
        {
            Log("SaveAsync: START");
            if (Offerte is null) { Log("SaveAsync: Offerte null -> return"); return; }

            try
            {
                using var db = _factory.CreateDbContext();

                Offerte.Regels.Clear();
                foreach (var r in Regels) Offerte.Regels.Add(r);
                Log($"SaveAsync: Offerte.Regels synced. Count={Offerte.Regels.Count}");

                PrepareOfferteForSave(db);

                if (Offerte.Id == 0)
                {
                    db.Offertes.Add(Offerte);
                    Log("SaveAsync: Offerte added (new).");
                }
                else
                {
                    db.Offertes.Update(Offerte);
                    Log($"SaveAsync: Offerte updated (Id={Offerte.Id}).");
                }

                var saved = await db.SaveChangesAsync();
                Log($"SaveAsync: SaveChanges -> {saved} entries affected. Offerte.Id={Offerte.Id}");
            }
            catch (Exception ex)
            {
                Log($"SaveAsync: ERROR {ex}");
                Foutmelding = ex.InnerException?.Message ?? ex.Message;
                return;
            }
            finally
            {
                Log("SaveAsync: END");
            }

            // 🔄 Reload met nieuwe context
            using (var db2 = _factory.CreateDbContext())
            {
                Log("SaveAsync: Reload START (fresh context)");
                var loaded = await db2.Offertes
                    .Include(o => o.Regels).ThenInclude(r => r.TypeLijst)
                    .Include(o => o.Regels).ThenInclude(r => r.Glas)
                    .Include(o => o.Regels).ThenInclude(r => r.PassePartout1)
                    .Include(o => o.Regels).ThenInclude(r => r.PassePartout2)
                    .Include(o => o.Regels).ThenInclude(r => r.DiepteKern)
                    .Include(o => o.Regels).ThenInclude(r => r.Opkleven)
                    .Include(o => o.Regels).ThenInclude(r => r.Rug)
                    .FirstAsync(x => x.Id == Offerte!.Id);

                Offerte = loaded;
                Regels = new ObservableCollection<OfferteRegel>(loaded.Regels);
                SelectedRegel = Regels.FirstOrDefault();
                Log($"SaveAsync: Reloaded. Regels={Regels.Count}, Totals=Ex:{OfferteEx} Btw:{OfferteBtw} Incl:{OfferteIncl}");
                RefreshOfferteTotals();
            }
        }


        // ───────── Legacy-code (proxy op de geselecteerde regel) ─────────
        public string? LegacyCode
        {
            get => SelectedRegel?.LegacyCode;
            set
            {
                if (SelectedRegel == null) { Log("LegacyCode.set: SelectedRegel null"); return; }
                if (SelectedRegel.LegacyCode != value)
                {
                    Log($"LegacyCode.set: '{SelectedRegel.LegacyCode}' -> '{value}'");
                    SelectedRegel.LegacyCode = value;
                    OnPropertyChanged();
                }
            }
        }

        // Helpers voor coderen/decoderen (zelfde als vroeger, compact gehouden)
        private static char Encode(int? idx)
        {
#if DEBUG
            Console.WriteLine($"[{DateTime.Now:O}] [OfferteVM] Encode({idx})");
#endif
            if (!idx.HasValue || idx.Value <= 0) return '0';
            if (idx.Value <= 9) return (char)('0' + idx.Value);
            int k = idx.Value - 10; // 0..10 => A..K
            if (k < 0 || k > 10) throw new ArgumentOutOfRangeException(nameof(idx));
            return (char)('A' + k);
        }
        private static int? Decode(char c)
        {
#if DEBUG
            Console.WriteLine($"[{DateTime.Now:O}] [OfferteVM] Decode('{c}')");
#endif
            if (c == '0') return (int?)null;
            if (c >= '1' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'K') return 10 + (c - 'A');
            if (c >= 'a' && c <= 'k') return 10 + (c - 'a');
            throw new ArgumentException($"Ongeldig teken: {c}");
        }

        // ───────── Commands ─────────
        [RelayCommand]
        private async Task ApplyLegacyCodeAsync()
        {
            Log("ApplyLegacyCodeAsync: START");
            if (SelectedRegel is null) { Log("ApplyLegacyCodeAsync: SelectedRegel null"); return; }
            if (string.IsNullOrWhiteSpace(LegacyCode)) { Log("ApplyLegacyCodeAsync: LegacyCode empty"); return; }

            try
            {
                var code = LegacyCode.Trim();
                Log($"ApplyLegacyCodeAsync: code='{code}' (len={code.Length})");
                if (code.Length < 6)
                    throw new ArgumentException("Code vereist 6 tekens (G P P D O R).");

                using var db = _factory.CreateDbContext();

                int gId = await db.AfwerkingsGroepen.Where(x => x.Code == 'G').Select(x => x.Id).FirstAsync();
                int pId = await db.AfwerkingsGroepen.Where(x => x.Code == 'P').Select(x => x.Id).FirstAsync();
                int dId = await db.AfwerkingsGroepen.Where(x => x.Code == 'D').Select(x => x.Id).FirstAsync();
                int oId = await db.AfwerkingsGroepen.Where(x => x.Code == 'O').Select(x => x.Id).FirstAsync();
                int rId = await db.AfwerkingsGroepen.Where(x => x.Code == 'R').Select(x => x.Id).FirstAsync();

                Log($"ApplyLegacyCodeAsync: groepIds G={gId}, P={pId}, D={dId}, O={oId}, R={rId}");

                int?[] idx = code.Take(6).Select(Decode).ToArray();
                Log($"ApplyLegacyCodeAsync: indices = [{string.Join(",", idx.Select(x => x?.ToString() ?? "null"))}]");

                async Task<AfwerkingsOptie?> FindAsync(int groepId, int? volg)
                {
                    if (!volg.HasValue) return null;
                    var item = await db.AfwerkingsOpties.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.AfwerkingsGroepId == groepId && a.Volgnummer == volg.Value);
                    Log($"ApplyLegacyCodeAsync: FindAsync(groepId={groepId}, volg={volg}) -> {(item == null ? "null" : $"{item.Id}:{item.Naam}")}");
                    return item;
                }

                SelectedRegel.Glas = await FindAsync(gId, idx[0]); SelectedRegel.GlasId = SelectedRegel.Glas?.Id;
                SelectedRegel.PassePartout1 = await FindAsync(pId, idx[1]); SelectedRegel.PassePartout1Id = SelectedRegel.PassePartout1?.Id;
                SelectedRegel.PassePartout2 = await FindAsync(pId, idx[2]); SelectedRegel.PassePartout2Id = SelectedRegel.PassePartout2?.Id;
                SelectedRegel.DiepteKern = await FindAsync(dId, idx[3]); SelectedRegel.DiepteKernId = SelectedRegel.DiepteKern?.Id;
                SelectedRegel.Opkleven = await FindAsync(oId, idx[4]); SelectedRegel.OpklevenId = SelectedRegel.Opkleven?.Id;
                SelectedRegel.Rug = await FindAsync(rId, idx[5]); SelectedRegel.RugId = SelectedRegel.Rug?.Id;

                Log($"ApplyLegacyCodeAsync: Set IDs G={SelectedRegel.GlasId}, P1={SelectedRegel.PassePartout1Id}, P2={SelectedRegel.PassePartout2Id}, D={SelectedRegel.DiepteKernId}, O={SelectedRegel.OpklevenId}, R={SelectedRegel.RugId}");

                OnPropertyChanged(nameof(SelectedRegel));
                Log("ApplyLegacyCodeAsync: UI notified for SelectedRegel change");
            }
            catch (Exception ex)
            {
                Log($"ApplyLegacyCodeAsync: ERROR {ex}");
                Foutmelding = ex.Message;
            }
            finally
            {
                Log("ApplyLegacyCodeAsync: END");
            }
        }

        [RelayCommand]
        private void GenerateLegacyCode()
        {
            Log("GenerateLegacyCode: START");
            if (SelectedRegel is null) { Log("GenerateLegacyCode: SelectedRegel null"); return; }

            var code = new[]
            {
                Encode(SelectedRegel.Glas?.Volgnummer),
                Encode(SelectedRegel.PassePartout1?.Volgnummer),
                Encode(SelectedRegel.PassePartout2?.Volgnummer),
                Encode(SelectedRegel.DiepteKern?.Volgnummer),
                Encode(SelectedRegel.Opkleven?.Volgnummer),
                Encode(SelectedRegel.Rug?.Volgnummer)
            };
            var result = new string(code);
            Log($"GenerateLegacyCode: result='{result}'");
            LegacyCode = result;
            Log("GenerateLegacyCode: END");
        }

        [RelayCommand]
        private void GaTerug()
        {
            Log("GaTerug: navigating to 'Home'");
            NavigatieGevraagd?.Invoke("Home");
        }
    }
}
