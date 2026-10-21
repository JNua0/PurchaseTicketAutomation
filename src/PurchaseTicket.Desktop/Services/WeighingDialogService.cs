using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;
using PurchaseTicket.Desktop.Views.VehicleWeighing;

namespace PurchaseTicket.Desktop.Services;

public class WeighingDialogService : IWeighingDialogService
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;

    // Constructor

    public WeighingDialogService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Metodos publicos

    // Cada apertura crea una ventana y un formulario nuevos, sin datos anteriores
    public WeighingRegistration? ShowCreateDialog()
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<CreateWeighingWindow>();

        var viewModel = (CreateWeighingViewModel)window.DataContext;

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true
            ? viewModel.Registration
            : null;
    }
}