
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Materials;

namespace PurchaseTicket.Desktop.Views.Materials;

public partial class MaterialFormWindow : Window
{
    public ICommand CancelCommand { get; }

    public MaterialFormWindow(MaterialFormViewModel viewModel)
    {
        CancelCommand = new RelayCommand(Cancel);

        InitializeComponent();

        DataContext = viewModel;

        viewModel.Saved += OnMaterialSaved;
    }

    // Cerrar la ventana cuando se guarda correctamente
    private void OnMaterialSaved(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

    // Cancelar la operacion
    private void Cancel()
    {
        if (DataContext is MaterialFormViewModel viewModel
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
        if (DataContext is MaterialFormViewModel viewModel
            && viewModel.IsSaving)
        {
            e.Cancel = true;
        }
    }
}
