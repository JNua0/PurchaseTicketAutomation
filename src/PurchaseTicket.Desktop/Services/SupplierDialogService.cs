
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Desktop.Views.Suppliers;
using PurchaseTicket.Desktop.ViewModels.Suppliers;

namespace PurchaseTicket.Desktop.Services;

public class SupplierDialogService : ISupplierDialogService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SupplierDialogService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public bool ShowCreateDialog()
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider.GetRequiredService<SupplierFormWindow>();

        var viewModel = (SupplierFormViewModel)window.DataContext;

        viewModel.InitializeForCreate();

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true;
    }


    public bool ShowEditDialog(
        int supplierId,
        string name,
        string? phoneNumber)
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<SupplierFormWindow>();

        var viewModel = (SupplierFormViewModel)window.DataContext;

        viewModel.InitializeForEdit(
            supplierId,
            name,
            phoneNumber);

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true;
    }

}
