using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using QuadroApp.ViewModels;

namespace QuadroApp.Views
{
    public partial class KlantenView : UserControl
    {
        public KlantenView()
        {
            InitializeComponent();
            DataContext = new KlantenViewModel(AppServices.Db);
        }

   
    }
}
