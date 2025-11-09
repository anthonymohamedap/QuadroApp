using Avalonia.Controls;
using Avalonia.Interactivity;
using QuadroApp.Model;

namespace QuadroApp.Views
{
    public partial class KlantDialog : Window
    {
        // 🔸 Mag null zijn totdat gebruiker op "Opslaan" klikt.
        public Klant? Result { get; private set; }

        public KlantDialog()
        {
            InitializeComponent();
        }

        public KlantDialog(Klant klant) : this()
        {
            // DataContext bepaalt de bindings in XAML
            DataContext = new Klant
            {
                Id = klant.Id,
                Voornaam = klant.Voornaam,
                Achternaam = klant.Achternaam,
                Email = klant.Email,
                Telefoon = klant.Telefoon,
                Straat = klant.Straat,
                Nummer = klant.Nummer,
                Postcode = klant.Postcode,
                Gemeente = klant.Gemeente
            };
        }

        private void Annuleren_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private void Opslaan_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is Klant klant)
                Result = klant;

            Close(Result);
        }
    }
}
