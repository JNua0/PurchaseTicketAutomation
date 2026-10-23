using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.UseCases.Suppliers.Get;
using PurchaseTicket.Desktop.ViewModels.Suppliers;
using PurchaseTicket.Desktop.Views.Suppliers;

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

    public bool ShowEditDialog(SupplierDto supplier)
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<SupplierFormWindow>();

        var viewModel = (SupplierFormViewModel)window.DataContext;

        viewModel.InitializeForEdit(supplier);

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true;
    }
}
