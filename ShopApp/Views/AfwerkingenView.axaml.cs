using Avalonia;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuadroApp.Data;
using QuadroApp.ViewModels;

namespace QuadroApp.Views
{
    public partial class AfwerkingenView : UserControl
    {
        public AfwerkingenView()
        {
            InitializeComponent();
            var factory = ((App)Application.Current!).Services.GetRequiredService<IDbContextFactory<AppDbContext>>();

            DataContext = new AfwerkingenViewModel(factory);
        }
    }
}
