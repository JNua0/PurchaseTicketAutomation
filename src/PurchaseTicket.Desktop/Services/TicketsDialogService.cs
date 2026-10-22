using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
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

    // Se abre desde el detalle, que queda como su ventana duena
    public bool ShowCorrectDialog(PurchaseTicketDetail ticket)
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<CorrectTicketWindow>();

        var viewModel = (CorrectTicketViewModel)window.DataContext;

        viewModel.Initialize(ticket);

        window.Owner = System.Windows.Application.Current.Windows
            .OfType<TicketDetailWindow>()
            .FirstOrDefault(w => w.IsActive)
            ?? System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true;
    }
}