using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;
using PurchaseTicket.Desktop.Views.VehicleWeighing;

namespace PurchaseTicket.Desktop.Services;

public class WeighingDialogService : IWeighingDialogService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public WeighingDialogService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Mostrar el formulario para registrar un nuevo pesaje
    public bool ShowCreateDialog()
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<CreateWeighingWindow>();

        var viewModel = (CreateWeighingViewModel)window.DataContext;

        viewModel.Initialize();

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true;
    }
}
