using System.Windows;
using System.ComponentModel;
using PurchaseTicket.Desktop.ViewModels.Suppliers;

namespace PurchaseTicket.Desktop.Views.Suppliers;

public partial class SupplierFormWindow : Window
{
    public SupplierFormWindow(SupplierFormViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        viewModel.Saved += OnSupplierSaved;
    }

    private void OnSupplierSaved(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

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