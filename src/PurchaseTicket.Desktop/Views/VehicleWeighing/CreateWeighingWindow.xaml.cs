using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.Views.VehicleWeighing;

public partial class CreateWeighingWindow : Window
{
    public ICommand CancelCommand { get; }

    public CreateWeighingWindow(CreateWeighingViewModel viewModel)
    {
        CancelCommand = new RelayCommand(Cancel);

        InitializeComponent();

        DataContext = viewModel;
    }

    // Cargar catalogos al abrir el formulario
    private async void Window_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is not CreateWeighingViewModel viewModel)
            return;

        await viewModel.LoadCatalogsAsync();
    }

    // Cerrar el formulario
    private void Cancel()
    {
        if (DataContext is not CreateWeighingViewModel viewModel)
            return;

        if (!viewModel.CanCloseForm)
            return;

        // Notificar si el ticket fue registrado
        DialogResult = viewModel.IsRegistered;
    }

    // Impedir cerrar durante una operacion
    private void Window_Closing(
        object? sender,
        CancelEventArgs e)
    {
        if (DataContext is not CreateWeighingViewModel viewModel)
            return;

        if (!viewModel.CanCloseForm)
        {
            e.Cancel = true;
            return;
        }

        // Conservar el resultado incluso al cerrar con la X
        DialogResult = viewModel.IsRegistered;
    }
}
