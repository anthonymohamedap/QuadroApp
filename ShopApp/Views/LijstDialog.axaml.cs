using Avalonia.Controls;
using Avalonia.Interactivity;
using QuadroApp.Model;
using Microsoft.EntityFrameworkCore;
using QuadroApp.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuadroApp.Views
{
    public partial class LijstDialog : Window
    {
        public bool Result { get; private set; } = false;

        public LijstDialog()
        {
            InitializeComponent();
        }

        public LijstDialog(TypeLijst lijst) : this()
        {
            DataContext = lijst;
        }

        private void Annuleer_Click(object? sender, RoutedEventArgs e)
        {
            Result = false;
            Close(false);
        }

        private void Opslaan_Click(object? sender, RoutedEventArgs e)
        {
            Result = true;
            Close(true);
        }

    }


}
