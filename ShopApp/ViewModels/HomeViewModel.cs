using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;

namespace QuadroApp.ViewModels
{
    public class HomeViewModel
    {
        public ICommand OpenKlantenCommand { get; }
        public ICommand OpenLijstenCommand { get; }
        public ICommand OpenOffertesCommand { get; }
        public ICommand OpenAfwerkingsOptiesCommand { get; }


        public event Action<string>? NavigatieGevraagd; // event dat MainWindow opvangt

        public HomeViewModel()
        {
            OpenKlantenCommand = new RelayCommand(() => NavigatieGevraagd?.Invoke("Klanten"));
            OpenLijstenCommand = new RelayCommand(() => NavigatieGevraagd?.Invoke("Lijsten"));
            OpenOffertesCommand = new RelayCommand(() => NavigatieGevraagd?.Invoke("Offertes"));
            OpenAfwerkingsOptiesCommand = new RelayCommand(() => NavigatieGevraagd?.Invoke("Afwerkingen"));
        }
    }
}
