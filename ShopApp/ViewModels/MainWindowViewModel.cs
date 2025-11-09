using CommunityToolkit.Mvvm.ComponentModel;
using QuadroApp.Views;

namespace QuadroApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty] private object huidigeView;

        private readonly HomeView _homeView;
        private readonly KlantenView _klantenView;
        private readonly LijstenView _lijstenView;
        private readonly AfwerkingenView _afwerkingenView;
        private readonly OfferteView _offerteView;


        public MainWindowViewModel()
        {
            // Maak views aan
            _homeView = new HomeView();
            _klantenView = new KlantenView();
            _lijstenView = new LijstenView();
            _afwerkingenView = new AfwerkingenView();
            _offerteView = new OfferteView();

            // Verbind events vanuit ViewModels
            if (_homeView.DataContext is HomeViewModel homeVm)
                homeVm.NavigatieGevraagd += NavigeerNaar;

            if (_lijstenView.DataContext is LijstenViewModel lijstenVm)
                lijstenVm.NavigatieGevraagd += NavigeerNaar;

            if (_klantenView.DataContext is KlantenViewModel klantenVm)
                klantenVm.NavigatieGevraagd += NavigeerNaar;


            if (_klantenView.DataContext is AfwerkingenViewModel AfwerkingVm)
                AfwerkingVm.NavigatieGevraagd += NavigeerNaar;

            if (_offerteView.DataContext is OfferteViewModel OfferteVm)
                OfferteVm.NavigatieGevraagd += NavigeerNaar;




            // Startpagina
            HuidigeView = _homeView;
        }

        private void NavigeerNaar(string pagina)
        {
            switch (pagina)
            {
                case "Klanten":
                    HuidigeView = _klantenView;
                    break;
                case "Lijsten":
                    HuidigeView = _lijstenView;
                    break;
                case "Home":
                default:
                    HuidigeView = _homeView;
                    break;
                case "Afwerkingen":
                    HuidigeView = _afwerkingenView;
                    break;
                case "Offertes":
                    HuidigeView = _offerteView;
                    break;
            }
        }
    }
}
