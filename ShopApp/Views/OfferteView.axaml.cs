using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuadroApp.Data;
using QuadroApp.ViewModels;

namespace QuadroApp.Views
{
    public partial class OfferteView : UserControl
    {
        public OfferteView()
        {
            InitializeComponent();

            var factory = ((App)Application.Current!).Services
                .GetRequiredService<IDbContextFactory<AppDbContext>>();
            DataContext = new OfferteViewModel(factory);
        }

        // Kalender openen en keuze terugzetten
        private async void OpenPlanningCalendar_Click(object? sender, RoutedEventArgs e)
        {
            var win = new PlanningCalendarWindow();

            win.PlanningChosen += (s, args) =>
            {
                if (DataContext is OfferteViewModel vm)
                {
                    var start = args.StartLocal;
                    vm.PlanDatum = new System.DateTimeOffset(start);
                    vm.PlanTijd = start.TimeOfDay;
                    vm.PlanDuurMinuten = args.DurationMin;
                }
                (s as Window)?.Close();
            };

            if (win.DataContext is PlanningCalendarViewModel pvm)
                await pvm.LoadAsync();

            if (this.VisualRoot is Window owner)
                await win.ShowDialog(owner);
            else
                win.Show();
        }
    }
}
