using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Layout;
using PurchaseTicket.Desktop.ViewModels.Tickets;

namespace PurchaseTicket.Desktop.Views.Tickets;

public partial class CorrectTicketWindow : Window
{
    // Campos

    private readonly CorrectTicketViewModel _viewModel;

    // Constructor

    public CorrectTicketWindow(CorrectTicketViewModel viewModel)
    {
        CancelCommand = new RelayCommand(Cancel);

        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        viewModel.Saved += OnSaved;
        Loaded += OnLoaded;

        DialogPlacement.KeepCenteredOnOwner(this);
    }

    // Propiedades

    public ICommand CancelCommand { get; }

    // Metodos privados

    // Los catalogos se cargan ya abierta, solo si se pueden corregir proveedor y material
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadCatalogsAsync();
    }

    // Cerrar la ventana cuando la correccion se guarda
    private void OnSaved(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel()
    {
        if (_viewModel.IsSaving)
            return;

        DialogResult = false;
    }

    // Evitar cerrar con la X mientras se guarda
    private void Window_Closing(
        object? sender,
        CancelEventArgs e)
    {
        if (_viewModel.IsSaving)
            e.Cancel = true;
    }
}