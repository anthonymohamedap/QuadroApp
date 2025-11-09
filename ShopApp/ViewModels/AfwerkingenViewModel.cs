// QuadroApp/ViewModels/AfwerkingenViewModel.cs
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuadroApp.Data;
using QuadroApp.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace QuadroApp.ViewModels
{
    public partial class AfwerkingenViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public event Action<string>? NavigatieGevraagd;

        // ─────────────────────────────────────────────────────────────
        // State
        // ─────────────────────────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<AfwerkingsGroep> groepen = new();
        [ObservableProperty] private AfwerkingsGroep? selectedGroep;

        [ObservableProperty] private ObservableCollection<AfwerkingsOptie> opties = new();
        [ObservableProperty] private ObservableCollection<AfwerkingsOptie> filteredOpties = new();

        private bool isSynchronizing;
        private AfwerkingsOptie? selectedOptie;
        public AfwerkingsOptie? SelectedOptie
        {
            get => selectedOptie;
            set
            {
                if (SetProperty(ref selectedOptie, value))
                {
                    DeleteAsyncCommand.NotifyCanExecuteChanged();
                    SyncSelectedOptieBindings();
                    OnPropertyChanged(nameof(PreviewPrijsText));
                    OnPropertyChanged(nameof(HeeftSelectie));
                }
            }
        }

        [ObservableProperty] private ObservableCollection<Leverancier> leveranciers = new();
        [ObservableProperty] private Leverancier? selectedLeverancier;

        [ObservableProperty] private string? zoekterm;
        [ObservableProperty] private string? foutmelding;
        [ObservableProperty] private bool hasChanges;
        [ObservableProperty] private string status = "";
        [ObservableProperty] private decimal previewBreedteCm = 30;
        [ObservableProperty] private decimal previewHoogteCm = 40;

        public string AantalOptiesText => $"{FilteredOpties.Count} opties";

        [ObservableProperty] private bool isDealerOptie;

        public bool HeeftSelectie => SelectedOptie is not null;

        // ─────────────────────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────────────────────
        public IAsyncRelayCommand RefreshAsyncCommand { get; }
        public IAsyncRelayCommand SaveAsyncCommand { get; }
        public IAsyncRelayCommand NewAsyncCommand { get; }
        public IAsyncRelayCommand DeleteAsyncCommand { get; }


        // ─────────────────────────────────────────────────────────────
        // Ctors
        // ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Ontwerp-/no-arg ctor: probeert factory uit DI te halen (zoals LijstenView doet).
        /// </summary>
        public AfwerkingenViewModel() : this(
            ((App)Application.Current!).Services.GetRequiredService<IDbContextFactory<AppDbContext>>()
        )
        { }

        public AfwerkingenViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            RefreshAsyncCommand = new AsyncRelayCommand(RefreshAsync);
            SaveAsyncCommand = new AsyncRelayCommand(SaveAsync);
            NewAsyncCommand = new AsyncRelayCommand(NewAsync);
            DeleteAsyncCommand = new AsyncRelayCommand(DeleteAsync, CanDelete);

            _ = LoadAsync();
        }

        [RelayCommand]
        private void GaTerug()
        {
            NavigatieGevraagd?.Invoke("Home");
        }

        // ─────────────────────────────────────────────────────────────
        // Data access (altijd nieuwe DbContext per actie)
        // ─────────────────────────────────────────────────────────────
        private async Task LoadAsync()
        {
            try
            {
                Foutmelding = null;
                Status = "Laden…";

                using (var db = _factory.CreateDbContext())
                {
                    Groepen = new ObservableCollection<AfwerkingsGroep>(
                        await db.AfwerkingsGroepen.AsNoTracking()
                            .OrderBy(g => g.Code)
                            .ToListAsync());

                    Leveranciers = new ObservableCollection<Leverancier>(
                        await db.Leveranciers.AsNoTracking()
                            .OrderBy(l => l.Naam)
                            .ToListAsync());
                }

                SelectedGroep ??= Groepen.FirstOrDefault();

                await LoadOptiesAsync();
                Status = "Klaar";
            }
            catch (Exception ex)
            {
                Foutmelding = $"Fout bij laden: {ex.Message}";
                Status = "Fout";
            }
        }

        private async Task LoadOptiesAsync()
        {
            try
            {
                using var db = _factory.CreateDbContext();

                var query = db.AfwerkingsOpties
                    .AsNoTracking()
                    .Include(o => o.Leverancier)
                    .Include(o => o.AfwerkingsGroep);

                if (SelectedGroep != null)
                    query = query.Where(o => o.AfwerkingsGroepId == SelectedGroep.Id);

                var opties = await query
                    .OrderBy(o => o.Volgnummer)
                    .ThenBy(o => o.Naam)
                    .ToListAsync();

                Opties = new ObservableCollection<AfwerkingsOptie>(opties);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                Foutmelding = $"Fout bij laden van opties: {ex.Message}";
            }
        }

        private void ApplyFilter()
        {
            var vorigeSelectieId = SelectedOptie?.Id;

            var src = Opties.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(Zoekterm))
            {
                var t = Zoekterm.Trim().ToLowerInvariant();
                src = src.Where(o =>
                    o.Volgnummer.ToString().Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    (o.Naam ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase));
            }
            FilteredOpties = new ObservableCollection<AfwerkingsOptie>(src);
            OnPropertyChanged(nameof(AantalOptiesText));

            var nieuweSelectie = vorigeSelectieId.HasValue
                ? FilteredOpties.FirstOrDefault(x => x.Id == vorigeSelectieId.Value)
                : FilteredOpties.FirstOrDefault();

            SelectedOptie = nieuweSelectie;
        }
        private void SyncSelectedOptieBindings()
        {
            isSynchronizing = true;
            try
            {
                if (selectedOptie is null)
                {
                    SelectedLeverancier = null;
                    IsDealerOptie = false;
                    return;
                }

                // Controleer of dit een dealeroptie is (DLR)
                IsDealerOptie = string.Equals(selectedOptie.Leverancier?.Code, "DLR", StringComparison.OrdinalIgnoreCase);

                // Stel de geselecteerde leverancier in
                SelectedLeverancier = selectedOptie.LeverancierId.HasValue
                    ? Leveranciers.FirstOrDefault(l => l.Id == selectedOptie.LeverancierId.Value)
                    : null;
            }
            finally
            {
                isSynchronizing = false;
            }
        }


        public string PreviewPrijsText
        {
            get
            {
                var o = selectedOptie;
                if (o == null) return "Selecteer een optie voor preview.";
                var m2 = (PreviewBreedteCm * PreviewHoogteCm) / 10_000m;
                if (m2 <= 0) return "Afmetingen ongeldig.";
                var kost = o.KostprijsPerM2 * m2 + o.VasteKost;
                var afval = kost * (o.AfvalPercentage / 100m);
                var excl = (kost + afval) * (1m + o.WinstMarge);
                return $"Preview: € {excl:F2} excl. btw (voor {PreviewBreedteCm}×{PreviewHoogteCm} cm)";
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────────────────────
        private async Task RefreshAsync() => await LoadAsync();

        private async Task SaveAsync()
        {
            try
            {
                Foutmelding = null;

                if (selectedOptie != null)
                {
                    using var db = _factory.CreateDbContext();
                    selectedOptie.LeverancierId = SelectedLeverancier?.Id;
                    if (SelectedLeverancier is not null)
                    {
                        selectedOptie.Leverancier = SelectedLeverancier;
                    }
                    db.Update(selectedOptie);
                    await db.SaveChangesAsync();
                }

                HasChanges = false;
                await LoadOptiesAsync();
            }
            catch (Exception ex)
            {
                Foutmelding = $"Opslaan mislukt: {ex.Message}";
            }
        }

        private async Task NewAsync()
        {
            if (SelectedGroep is null) return;

            using var db = _factory.CreateDbContext();

            var volgendNummer = await db.AfwerkingsOpties
                .Where(o => o.AfwerkingsGroepId == SelectedGroep.Id)
                .Select(o => o.Volgnummer)
                .DefaultIfEmpty(0)
                .MaxAsync() + 1;

            var nieuw = new AfwerkingsOptie
            {
                AfwerkingsGroepId = SelectedGroep.Id,
                Naam = "Nieuwe optie",
                Volgnummer = volgendNummer,
                WinstMarge = 0.25m,
                AfvalPercentage = 0m,
                KostprijsPerM2 = 0m,
                VasteKost = 0m,
                WerkMinuten = 0
            };

            db.AfwerkingsOpties.Add(nieuw);
            await db.SaveChangesAsync();

            await LoadOptiesAsync();
            SelectedOptie = FilteredOpties.FirstOrDefault(x => x.Id == nieuw.Id);
            HasChanges = true;
        }

        private bool CanDelete() => selectedOptie is not null;

        private async Task DeleteAsync()
        {
            if (selectedOptie == null) return;

            using var db = _factory.CreateDbContext();
            db.AfwerkingsOpties.Remove(selectedOptie);
            await db.SaveChangesAsync();

            await LoadOptiesAsync();
            HasChanges = true;
        }

        partial void OnZoektermChanged(string? value) => ApplyFilter();

        partial void OnSelectedGroepChanged(AfwerkingsGroep? value)
        {
            _ = LoadOptiesAsync();
        }

        partial void OnPreviewBreedteCmChanged(decimal value) => OnPropertyChanged(nameof(PreviewPrijsText));

        partial void OnPreviewHoogteCmChanged(decimal value) => OnPropertyChanged(nameof(PreviewPrijsText));

        partial void OnSelectedLeverancierChanged(Leverancier? value)
        {
            if (selectedOptie is null || isSynchronizing) return;
            selectedOptie.LeverancierId = value?.Id;
            selectedOptie.Leverancier = value;
            HasChanges = true;
        }
    }
}
