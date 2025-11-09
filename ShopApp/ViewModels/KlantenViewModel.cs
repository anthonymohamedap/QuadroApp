using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuadroApp.Data;
using QuadroApp.Model;
using QuadroApp.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace QuadroApp.ViewModels
{
    /// <summary>
    /// CRUD ViewModel voor Klantenbeheer.
    /// </summary>
    public partial class KlantenViewModel : ObservableObject
    {
        private readonly AppDbContext _db;

        [ObservableProperty] private ObservableCollection<Klant> klanten = new();
        [ObservableProperty] private ObservableCollection<Klant> filteredKlanten = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private Klant? selectedKlant;

        [ObservableProperty] private string? foutmelding;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string? zoekterm;
        [ObservableProperty] private bool hasChanges;
        public event Action<string>? NavigatieGevraagd;


        public KlantenViewModel(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db), "Databasecontext is null");
            _ = LoadAsync();
            PropertyChanged += OnVmPropertyChanged;
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Zoekterm))
                ApplyFilter();
        }

        /// <summary>Laadt alle klanten uit de database (tracking AAN zodat edits opgeslagen worden).</summary>
        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                Foutmelding = null;

                var list = await _db.Klanten
                    // ⚠️ GEEN .AsNoTracking() hier → anders slaat EF wijzigingen niet op
                    .OrderBy(k => k.Achternaam)
                    .ThenBy(k => k.Voornaam)
                    .ToListAsync();

                Klanten = new ObservableCollection<Klant>(list);
                FilteredKlanten = new ObservableCollection<Klant>(Klanten);
                OnPropertyChanged(nameof(FilteredKlanten));
            }
            catch (Exception ex)
            {
                Foutmelding = $"Fout bij laden: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GaTerug()
        {
            NavigatieGevraagd?.Invoke("Home");
        }
        /// <summary>Past live filtering toe op de lijst.</summary>
        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(Zoekterm))
            {
                FilteredKlanten = new ObservableCollection<Klant>(Klanten);
                return;
            }

            var t = Zoekterm.Trim().ToLowerInvariant();
            var q = Klanten.Where(k =>
                (k.Voornaam ?? "").ToLowerInvariant().Contains(t) ||
                (k.Achternaam ?? "").ToLowerInvariant().Contains(t) ||
                (k.Email ?? "").ToLowerInvariant().Contains(t) ||
                (k.Gemeente ?? "").ToLowerInvariant().Contains(t) ||
                (k.Straat ?? "").ToLowerInvariant().Contains(t) ||
                (k.Telefoon ?? "").ToLowerInvariant().Contains(t) ||
                (k.Opmerking ?? "").ToLowerInvariant().Contains(t));


            FilteredKlanten = new ObservableCollection<Klant>(q);
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadAsync();

        [RelayCommand]
        private async Task NewAsync()
        {
            var dialog = new KlantDialog(new Klant());
            var owner = TryGetMainWindow();

            Klant? result = owner is not null
                ? await dialog.ShowDialog<Klant?>(owner)
                : await ShowNonModalAndAwaitResultAsync(dialog);

            if (result is not null)
            {
                _db.Klanten.Add(result);        // EF gaat deze tracken
                Klanten.Add(result);
                FilteredKlanten.Add(result);
                HasChanges = true;
            }
        }

        // CanExecute bepaalt of de knoppen actief zijn (zonder converters)
        private bool CanEdit() => SelectedKlant is not null;
        private bool CanDelete() => SelectedKlant is not null;

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditAsync()
        {
            if (SelectedKlant is null) return;

            // Kopie tonen in dialog, daarna terugschrijven (avoid live-binding)
            var kopie = new Klant
            {
                Id = SelectedKlant.Id,
                Voornaam = SelectedKlant.Voornaam,
                Achternaam = SelectedKlant.Achternaam,
                Email = SelectedKlant.Email,
                Telefoon = SelectedKlant.Telefoon,
                Straat = SelectedKlant.Straat,
                Nummer = SelectedKlant.Nummer,
                Postcode = SelectedKlant.Postcode,
                Gemeente = SelectedKlant.Gemeente,
                Opmerking = SelectedKlant.Opmerking
            };

            var dialog = new KlantDialog(kopie);
            var owner = TryGetMainWindow();

            Klant? result = owner is not null
                ? await dialog.ShowDialog<Klant?>(owner)
                : await ShowNonModalAndAwaitResultAsync(dialog);

            if (result is null) return;

            // ⚙️ SelectedKlant is een EF-getrackte entiteit ⇒ wijzigingen worden opgeslagen
            SelectedKlant.Voornaam = result.Voornaam;
            SelectedKlant.Achternaam = result.Achternaam;
            SelectedKlant.Email = result.Email;
            SelectedKlant.Telefoon = result.Telefoon;
            SelectedKlant.Straat = result.Straat;
            SelectedKlant.Nummer = result.Nummer;
            SelectedKlant.Postcode = result.Postcode;
            SelectedKlant.Gemeente = result.Gemeente;
            SelectedKlant.Opmerking = result.Opmerking;

            // UI verversen
            OnPropertyChanged(nameof(SelectedKlant));
            OnPropertyChanged(nameof(FilteredKlanten));
            HasChanges = true;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                Foutmelding = null;
                await _db.SaveChangesAsync(); // werkt nu omdat EF tracking aan staat
                HasChanges = false;
                await LoadAsync(); // ververs view
            }
            catch (Exception ex)
            {
                Foutmelding = $"❌ Opslaan mislukt: {ex.Message}";
            }
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private void Delete()
        {
            if (SelectedKlant is null) return;

            try
            {
                _db.Klanten.Remove(SelectedKlant);
                Klanten.Remove(SelectedKlant);
                FilteredKlanten.Remove(SelectedKlant);
                SelectedKlant = null;
                HasChanges = true;
            }
            catch (Exception ex)
            {
                Foutmelding = $"❌ Verwijderen mislukt: {ex.Message}";
            }
        }

        public void MarkDirty() => HasChanges = true;

        // ---------- helpers ----------
        private static Window? TryGetMainWindow()
        {
            return (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }

        /// <summary>
        /// Valt terug op niet-modale weergave als er geen owner is, maar wacht toch tot sluit.
        /// </summary>
        private static Task<Klant?> ShowNonModalAndAwaitResultAsync(KlantDialog dialog)
        {
            var tcs = new TaskCompletionSource<Klant?>();
            dialog.Closed += (_, __) => tcs.TrySetResult(dialog.Result);
            dialog.Show(); // geen owner → vermijdt 'PlatformImpl is null'
            return tcs.Task;
        }
    }
}
