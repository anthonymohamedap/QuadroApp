using Avalonia;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuadroApp.Data;
using QuadroApp.ViewModels;

namespace QuadroApp.Views
{
    public partial class LijstenView : UserControl
    {
        public LijstenView()
        {
            InitializeComponent();

            // Haal de factory uit DI
            var factory = ((App)Application.Current!).Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            DataContext = new LijstenViewModel(factory);
        }


    }
}
