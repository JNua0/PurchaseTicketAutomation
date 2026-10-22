using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Desktop.ViewModels.Tickets;
using PurchaseTicket.Desktop.Views.Tickets;

namespace PurchaseTicket.Desktop.Services;

public class TicketsDialogService : ITicketsDialogService
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;

    // Constructor

    public TicketsDialogService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Metodos publicos

    public bool ShowDetailDialog(int ticketId)
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<TicketDetailWindow>();

        var viewModel = (TicketDetailViewModel)window.DataContext;

        viewModel.Initialize(ticketId);

        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();

        return viewModel.HasChanges;
    }
}