using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Suppliers;

namespace PurchaseTicket.Desktop.Views.Suppliers;

public partial class SupplierFormWindow : Window
{
    public ICommand CancelCommand { get; }

    public SupplierFormWindow(SupplierFormViewModel viewModel)
    {
        // Configurar el comando de cancelacion
        CancelCommand = new RelayCommand(Cancel);

        InitializeComponent();

        DataContext = viewModel;

        viewModel.Saved += OnSupplierSaved;
    }

    // Cerrar la ventana cuando se guarda correctamente
    private void OnSupplierSaved(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

    // Cancelar la operacion
    private void Cancel()
    {
        if (DataContext is SupplierFormViewModel viewModel
            && viewModel.IsSaving)
        {
            return;
        }

        DialogResult = false;
    }

    // Evitar cerrar la ventana durante el guardado
    private void Window_Closing(
        object? sender,
        CancelEventArgs e)
    {
        if (DataContext is SupplierFormViewModel viewModel
            && viewModel.IsSaving)
        {
            e.Cancel = true;
        }
    }
}
