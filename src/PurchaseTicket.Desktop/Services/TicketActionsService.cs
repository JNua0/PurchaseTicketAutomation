using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Cancel;
using PurchaseTicket.Domain.Exceptions;

namespace PurchaseTicket.Desktop.Services;

public sealed class TicketActionsService : ITicketActionsService
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfirmationDialogService _confirmationDialogService;
    private readonly INotificationService _notifications;
    private readonly ITicketPrintService _printService;
    private readonly ILogger<TicketActionsService> _logger;

    // Constructor

    public TicketActionsService(
        IServiceScopeFactory scopeFactory,
        IConfirmationDialogService confirmationDialogService,
        INotificationService notifications,
        ITicketPrintService printService,
        ILogger<TicketActionsService> logger)
    {
        _scopeFactory = scopeFactory;
        _confirmationDialogService = confirmationDialogService;
        _notifications = notifications;
        _printService = printService;
        _logger = logger;
    }

    // Metodos publicos

    public async Task<CancelTicketOutcome> CancelAsync(int ticketId, string ticketNumber)
    {
        // "Volver" en lugar de "Cancelar" para no confundirlo con "Cancelar ticket"
        bool confirmed = _confirmationDialogService.Confirm(new ConfirmationRequest(
            $"¿Cancelar el ticket folio {ticketNumber}?",
            "El ticket queda como Cancelado: ya no se puede corregir ni reimprimir. " +
            "Esta acción no se puede deshacer.",
            "Cancelar ticket",
            CancelText: "Volver"));

        if (!confirmed)
            return CancelTicketOutcome.NotConfirmed;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            await scope.ServiceProvider
                .GetRequiredService<CancelPurchaseTicket>()
                .ExecuteAsync(new CancelPurchaseTicketCommand(ticketId));
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            _notifications.ShowError(ex.Message);

            return CancelTicketOutcome.Rejected;
        }
        catch (Exception ex)
        {
            _notifications.ShowError("No fue posible cancelar el ticket.");

            _logger.LogError(
                ex,
                "No fue posible cancelar el ticket {TicketId}.",
                ticketId);

            return CancelTicketOutcome.Failed;
        }

        _notifications.ShowSuccess($"Ticket folio {ticketNumber} cancelado.");

        return CancelTicketOutcome.Cancelled;
    }

    public async Task ReprintAsync(int ticketId, string ticketNumber)
    {
        // El servicio de impresion ya traduce sus fallas a un mensaje
        string? printWarning = await _printService.PrintAsync(ticketId);

        if (printWarning is null)
        {
            _notifications.ShowSuccess(
                $"Ticket folio {ticketNumber} enviado a impresión.");
        }
        else
        {
            _notifications.ShowWarning(
                $"No se imprimió el folio {ticketNumber}. {printWarning}");
        }
    }
}