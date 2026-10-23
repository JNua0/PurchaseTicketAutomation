using System.Windows;
using System.Windows.Controls;
using PurchaseTicket.Desktop.ViewModels.Home;

namespace PurchaseTicket.Desktop.Views.Home;

// La vista se crea cada vez que se entra a Inicio y se descarga al salir:
// asi el resumen se recarga al entrar y el temporizador solo corre mientras se ve
public partial class HomeView : UserControl
{
    // Constructor

    public HomeView()
    {
        InitializeComponent();
    }

    // Metodos privados

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is HomeViewModel viewModel)
            await viewModel.ActivateAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is HomeViewModel viewModel)
            viewModel.Deactivate();
    }
}
