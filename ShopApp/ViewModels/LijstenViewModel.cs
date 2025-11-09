using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuadroApp.Data;
using QuadroApp.Model;
using QuadroApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace QuadroApp.ViewModels
{
    public partial class LijstenViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        public event Action<string>? NavigatieGevraagd;

        [ObservableProperty] private ObservableCollection<TypeLijst> lijsten = new();
        [ObservableProperty] private ObservableCollection<TypeLijst> filteredLijsten = new();
        [ObservableProperty] private List<Leverancier> leveranciers = new();
        [ObservableProperty] private TypeLijst? geselecteerdeLijst;
        [ObservableProperty] private string zoekterm = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string? foutmelding;
        [ObservableProperty] private decimal breedte = 30m;
        [ObservableProperty] private decimal hoogte = 40m;
        [ObservableProperty] private decimal werkloon = 15m;

        partial void OnBreedteChanged(decimal value)
        {
            OnPropertyChanged(nameof(VerkoopPrijsPreview));
        }

        partial void OnHoogteChanged(decimal value)
        {
            OnPropertyChanged(nameof(VerkoopPrijsPreview));
        }


        public int AantalLijsten => FilteredLijsten.Count;

        public LijstenViewModel(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            PropertyChanged += OnPropertyChangedHandler;
            _ = LoadAsync();
        }

        private void OnPropertyChangedHandler(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Zoekterm))
                ApplyFilter();
        }

        public string VerkoopPrijsPreview =>
    GeselecteerdeLijst == null
        ? "Selecteer een lijst om prijs te berekenen"
        : $"💰 Geschatte verkoopprijs: € {BerekenPrijs(GeselecteerdeLijst, Breedte, Hoogte, werkloon):F2}";


        private static decimal BerekenPrijs(TypeLijst lijst, decimal breedteCm, decimal hoogteCm, decimal werkloon)
        {
            // Bereken totale lengte in meter
            var lengteMeter = ((breedteCm + hoogteCm) * 2m + 10m * (lijst.BreedteCm / 10m)) / 100m;

            var kost = lijst.PrijsPerMeter * lengteMeter;
            var marge = kost * lijst.WinstMargeFactor;
            var afval = kost * (lijst.AfvalPercentage / 100m);
            var werk = lijst.WerkMinuten > 0
                ? werkloon * (lijst.WerkMinuten / 60m)
                : 0m;

            return marge + afval + lijst.VasteKost;
        }


        // ───────────────────────────────
        // 📦 CRUD COMMANDS
        // ───────────────────────────────
        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                Console.WriteLine("Lijsten laden...");
                if (IsBusy) return;
                IsBusy = true;

                using var db = _contextFactory.CreateDbContext();

                // 🔎 LOG: waar zit mijn DB?
                var cs = db.Database.GetDbConnection().ConnectionString;
                Console.WriteLine($"[DB] ConnectionString: {cs}");
                try
                {
                    var dataSource = db.Database.GetDbConnection().DataSource;
                    Console.WriteLine($"[DB] DataSource path: {dataSource}");
                }
                catch { /* sommige providers */ }

                // (optioneel) forceer aanmaak – handig bij lege/nieuwe DB
                // await db.Database.EnsureCreatedAsync();

                // ✅ Haal leveranciers op
                Leveranciers = await db.Leveranciers.AsNoTracking().OrderBy(l => l.Naam).ToListAsync();
                Console.WriteLine($"[DB] Leveranciers: {Leveranciers.Count}");

                // ✅ Haal lijsten op
                var lijstenData = await db.TypeLijsten
                    .Include(l => l.Leverancier)
                    .AsNoTracking()
                    .OrderBy(l => l.Artikelnummer)
                    .ToListAsync();

                Console.WriteLine($"[DB] Gevonden lijsten: {lijstenData.Count}");

                foreach (var lijst in lijstenData)
                    lijst.AlleLeveranciers = Leveranciers;

                Lijsten = new ObservableCollection<TypeLijst>(lijstenData);
                FilteredLijsten = new ObservableCollection<TypeLijst>(Lijsten);

                OnPropertyChanged(nameof(AantalLijsten));
            }
            catch (Exception ex)
            {
                // ✔ log de échte fout naar de console
                Console.WriteLine($"[DB] FOUT bij laden lijsten: {ex}");
                Foutmelding = $"❌ Fout bij laden lijsten: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }


        private void ApplyFilter()
        {
            if (Lijsten.Count == 0) return;

            if (string.IsNullOrWhiteSpace(Zoekterm))
            {
                FilteredLijsten = new ObservableCollection<TypeLijst>(Lijsten);
            }
            else
            {
                var term = Zoekterm.Trim().ToLowerInvariant();
                var filtered = Lijsten.Where(l =>
                    (l.Artikelnummer ?? "").ToLowerInvariant().Contains(term) ||
                    (l.Beschrijving ?? "").ToLowerInvariant().Contains(term) ||
                    (l.LeverancierCode ?? "").ToLowerInvariant().Contains(term) ||
                    (l.Soort ?? "").ToLowerInvariant().Contains(term));

                FilteredLijsten = new ObservableCollection<TypeLijst>(filtered);
            }

            OnPropertyChanged(nameof(AantalLijsten));
        }
        [RelayCommand]
        private void GaTerug()
        {
            NavigatieGevraagd?.Invoke("Home");
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadAsync();

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                if (IsBusy) return;
                IsBusy = true;
                Foutmelding = null;

                if (GeselecteerdeLijst is null)
                {
                    Foutmelding = "⚠️ Geen lijst geselecteerd om op te slaan.";
                    return;
                }

                using var db = _contextFactory.CreateDbContext();

                // Neem het geselecteerde object, geef het 'nu', attach & markeer Modified
                var dto = GeselecteerdeLijst;
                dto.LaatsteUpdate = DateTime.Now;

                db.Attach(dto);
                db.Entry(dto).State = EntityState.Modified; // update alle kolommen van deze ene rij
                await db.SaveChangesAsync();

                await LoadAsync();
            }
            catch (Exception ex)
            {
                Foutmelding = $"❌ Opslaan mislukt: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }


        [RelayCommand]
        private async Task NewAsync()
        {
            var dialog = new LijstDialog(new TypeLijst());
            var owner = TryGetMainWindow();

            bool result = owner is not null
                ? await dialog.ShowDialog<bool>(owner)
                : await ShowNonModalAndAwaitResultAsync(dialog);

            if (!result) return;


            using var db = _contextFactory.CreateDbContext();
            db.TypeLijsten.Add((TypeLijst)dialog.DataContext!);
            await db.SaveChangesAsync();
            await LoadAsync();
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditAsync()
        {
            if (GeselecteerdeLijst is null) return;

            var dialog = new LijstDialog(GeselecteerdeLijst);
            var owner = TryGetMainWindow();

            bool result = owner is not null
                ? await dialog.ShowDialog<bool>(owner)
                : await ShowNonModalAndAwaitResultAsync(dialog);

            if (!result) return;
            using var db = _contextFactory.CreateDbContext();
            db.Update(GeselecteerdeLijst);
            await db.SaveChangesAsync();
            await LoadAsync();
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private async Task DeleteAsync()
        {
            if (GeselecteerdeLijst is null) return;

            using var db = _contextFactory.CreateDbContext();
            db.Remove(GeselecteerdeLijst);
            await db.SaveChangesAsync();
            await LoadAsync();
        }

        private bool CanEdit() => GeselecteerdeLijst is not null;
        private bool CanDelete() => GeselecteerdeLijst is not null;

        private static Window? TryGetMainWindow()
        {
            return (Avalonia.Application.Current?.ApplicationLifetime
                as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?
                .MainWindow;
        }

        private static Task<bool> ShowNonModalAndAwaitResultAsync(LijstDialog dialog)
        {
            var tcs = new TaskCompletionSource<bool>();
            dialog.Closed += (_, __) => tcs.TrySetResult(dialog.Result);
            dialog.Show();
            return tcs.Task;
        }
    }
}
