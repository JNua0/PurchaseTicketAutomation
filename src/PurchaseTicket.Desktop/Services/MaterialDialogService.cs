using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.UseCases.Materials.Get;
using PurchaseTicket.Desktop.ViewModels.Materials;
using PurchaseTicket.Desktop.Views.Materials;

namespace PurchaseTicket.Desktop.Services;

public class MaterialDialogService : IMaterialDialogService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MaterialDialogService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Mostrar formulario para registrar un material
    public string? ShowCreateDialog()
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<MaterialFormWindow>();

        var viewModel = (MaterialFormViewModel)window.DataContext;

        viewModel.InitializeForCreate();

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true ? viewModel.SavedName : null;
    }

    // Mostrar formulario para editar un material
    public string? ShowEditDialog(MaterialDto material)
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<MaterialFormWindow>();

        var viewModel = (MaterialFormViewModel)window.DataContext;

        viewModel.InitializeForEdit(material);

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true ? viewModel.SavedName : null;
    }
}
