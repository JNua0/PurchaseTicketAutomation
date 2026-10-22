using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Layout;
using PurchaseTicket.Desktop.ViewModels.Tickets;

namespace PurchaseTicket.Desktop.Views.Tickets;

public partial class TicketDetailWindow : Window
{
    // Campos

    private readonly TicketDetailViewModel _viewModel;

    // Constructor

    public TicketDetailWindow(TicketDetailViewModel viewModel)
    {
        CloseCommand = new RelayCommand(Close);

        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += OnLoaded;

        DialogPlacement.KeepCenteredOnOwner(this);
    }

    // Propiedades

    public ICommand CloseCommand { get; }

    // Metodos privados

    // Se carga ya abierta: la ventana aparece de inmediato con "Cargando ticket..."
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    // Evitar cerrar mientras imprime o cancela
    private void Window_Closing(
        object? sender,
        CancelEventArgs e)
    {
        if (_viewModel.IsBusy)
            e.Cancel = true;
    }
}