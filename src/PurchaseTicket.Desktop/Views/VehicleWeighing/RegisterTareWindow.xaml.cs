using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.Views.VehicleWeighing;

public partial class RegisterTareWindow : Window
{
    // Campos

    private readonly RegisterTareViewModel _viewModel;

    // Constructor

    public RegisterTareWindow(RegisterTareViewModel viewModel)
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

    // Cerrar la ventana cuando la tara se registra
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