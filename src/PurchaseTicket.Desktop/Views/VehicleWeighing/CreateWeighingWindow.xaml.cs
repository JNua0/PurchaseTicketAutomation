using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.Views.VehicleWeighing;

public partial class CreateWeighingWindow : Window
{
    // Campos

    private readonly CreateWeighingViewModel _viewModel;

    // Constructor

    public CreateWeighingWindow(CreateWeighingViewModel viewModel)
    {
        CancelCommand = new RelayCommand(Cancel);

        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        viewModel.Registered += OnRegistered;
    }

    // Propiedades

    public ICommand CancelCommand { get; }

    // Metodos privados

    // Los catalogos se cargan al abrir para mostrar los proveedores activos de ese momento
    private async void Window_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await _viewModel.LoadCatalogsAsync();
    }

    // Cerrar la ventana cuando el ticket se registra
    private void OnRegistered(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel()
    {
        if (_viewModel.IsBusy)
            return;

        DialogResult = false;
    }

    // Evitar cerrar con la X mientras se lee el indicador o se registra
    private void Window_Closing(
        object? sender,
        CancelEventArgs e)
    {
        if (_viewModel.IsBusy)
            e.Cancel = true;
    }
}