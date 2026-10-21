using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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
    public bool ShowCreateDialog()
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<MaterialFormWindow>();

        var viewModel = (MaterialFormViewModel)window.DataContext;

        viewModel.InitializeForCreate();

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true;
    }

    // Mostrar formulario para editar un material
    public bool ShowEditDialog(int materialId, string name)
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<MaterialFormWindow>();

        var viewModel = (MaterialFormViewModel)window.DataContext;

        viewModel.InitializeForEdit(materialId, name);

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true;
    }
}
