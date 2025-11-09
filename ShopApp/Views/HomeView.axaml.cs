using Avalonia.Controls;
using QuadroApp.ViewModels;

namespace QuadroApp.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            // ✅ ZET HIER DataContext
            DataContext = new HomeViewModel();
        }
    }
}
