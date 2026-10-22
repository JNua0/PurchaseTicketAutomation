using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Desktop.ViewModels.Amounts;
using PurchaseTicket.Desktop.Views.Amounts;

namespace PurchaseTicket.Desktop.Services;

public class AmountDialogService : IAmountDialogService
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;

    // Constructor

    public AmountDialogService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Metodos publicos

    // Cada apertura crea una ventana y un formulario nuevos, sin datos anteriores
    public AmountRegistration? ShowRegisterAmountDialog(AmountListItem ticket)
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<RegisterAmountWindow>();

        var viewModel = (RegisterAmountViewModel)window.DataContext;

        viewModel.Initialize(ticket);

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true
            ? viewModel.Registration
            : null;
    }
}