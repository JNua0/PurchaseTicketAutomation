using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Amounts;

namespace PurchaseTicket.Desktop.Views.Amounts;

public partial class RegisterAmountWindow : Window
{
    // Campos

    private readonly RegisterAmountViewModel _viewModel;

    // Constructor

    public RegisterAmountWindow(RegisterAmountViewModel viewModel)
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

    // Cerrar la ventana cuando el importe se registra
    private void OnRegistered(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel()
    {
        if (_viewModel.IsSaving)
            return;

        DialogResult = false;
    }

    // Evitar cerrar con la X mientras se registra
    private void Window_Closing(
        object? sender,
        CancelEventArgs e)
    {
        if (_viewModel.IsSaving)
            e.Cancel = true;
    }
}