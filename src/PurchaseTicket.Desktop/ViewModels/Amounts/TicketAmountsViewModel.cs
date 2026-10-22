using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Cancel;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.Exceptions;

namespace PurchaseTicket.Desktop.ViewModels.Amounts;

// Pantalla Importes: todos los tickets pendientes de importe, sin filtro de
// fechas, del que lleva mas tiempo esperando al mas reciente
public class TicketAmountsViewModel : PagedListViewModel<AmountListItem>
{
    // Campos

    private readonly INotificationService _notifications;
    private readonly IConfirmationDialogService _confirmationDialogService;
    private readonly TimeProvider _timeProvider;
    private int _pendingCount;

    // Constructor

    public TicketAmountsViewModel(
        IServiceScopeFactory scopeFactory,
        INotificationService notifications,
        IConfirmationDialogService confirmationDialogService,
        TimeProvider timeProvider,
        ILogger<TicketAmountsViewModel> logger)
        : base(scopeFactory, logger)
    {
        _notifications = notifications;
        _confirmationDialogService = confirmationDialogService;
        _timeProvider = timeProvider;

        CancelTicketCommand = new AsyncRelayCommand<AmountListItem>(
            CancelTicketAsync,
            CanActOn);
    }

    // Propiedades

    public string Description =>
        "Tickets con el pesaje terminado que esperan su importe";

    // Total pendiente, sin importar la busqueda
    public int PendingCount
    {
        get => _pendingCount;
        private set
        {
            if (SetProperty(ref _pendingCount, value))
                OnPropertyChanged(nameof(PendingCountText));
        }
    }

    public string PendingCountText =>
        PendingCount == 1
            ? "1 pendiente de importe"
            : $"{PendingCount} pendientes de importe";

    // Comandos

    public AsyncRelayCommand<AmountListItem> CancelTicketCommand { get; }

    // Propiedades protegidas

    protected override string LoadErrorMessage =>
        "No fue posible cargar los tickets pendientes de importe.";

    protected override string EmptyListMessage =>
        "No hay tickets pendientes de importe.";

    protected override string NoResultsMessage =>
        "No se encontraron tickets con la búsqueda aplicada.";

    // Metodos protegidos

    protected override async Task<PagedResult<AmountListItem>> FetchPageAsync(
        IServiceProvider services,
        int page,
        int pageSize,
        string? search)
    {
        var getTickets =
            services.GetRequiredService<GetPurchaseTickets>();

        var result = await getTickets.ExecuteAsync(
            PendingQuery(page, pageSize, search));

        // Sin busqueda el total ya es el pendiente; con busqueda se cuenta
        // aparte pidiendo una sola fila
        if (search is null)
        {
            PendingCount = result.TotalCount;
        }
        else
        {
            var pending = await getTickets.ExecuteAsync(
                PendingQuery(page: 1, pageSize: 1, search: null));

            PendingCount = pending.TotalCount;
        }

        var items = result.Items
            .Select(ticket => AmountListItem.From(ticket, _timeProvider))
            .ToList();

        return new PagedResult<AmountListItem>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    protected override void NotifyCommandsCanExecuteChanged()
    {
        base.NotifyCommandsCanExecuteChanged();

        CancelTicketCommand?.NotifyCanExecuteChanged();
    }

    // Metodos privados

    private static GetPurchaseTicketsQuery PendingQuery(
        int page,
        int pageSize,
        string? search)
    {
        return new GetPurchaseTicketsQuery(
            Page: page,
            PageSize: pageSize,
            Search: search,
            Status: TicketStatus.AmountPending,
            OldestFirst: true);
    }

    private bool CanActOn(AmountListItem ticket)
    {
        return ticket is not null && CanInteract();
    }

    // Mismo flujo que en Pesaje. Si la pantalla Tickets lo necesita tambien,
    // conviene moverlo a un servicio compartido
    private async Task CancelTicketAsync(AmountListItem ticket)
    {
        bool confirmed = _confirmationDialogService.Confirm(
            "Cancelar ticket",
            $"¿Deseas cancelar el ticket folio {ticket.TicketNumber}? " +
            "Esta acción no se puede deshacer.");

        if (!confirmed)
            return;

        IsBusy = true;

        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();

            await scope.ServiceProvider
                .GetRequiredService<CancelPurchaseTicket>()
                .ExecuteAsync(new CancelPurchaseTicketCommand(ticket.Id));
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            // Por ejemplo, otro equipo ya registro su importe: se recarga para mostrarlo
            _notifications.ShowError(ex.Message);
            IsBusy = false;

            await ReloadAfterActionAsync(
                CurrentPage,
                "No fue posible actualizar el listado.");

            return;
        }
        catch (Exception ex)
        {
            _notifications.ShowError("No fue posible cancelar el ticket.");

            Logger.LogError(
                ex,
                "No fue posible cancelar el ticket {TicketId}.",
                ticket.Id);

            return;
        }
        finally
        {
            IsBusy = false;
        }

        _notifications.ShowSuccess(
            $"Ticket folio {ticket.TicketNumber} cancelado.");

        await ReloadAfterActionAsync(
            CurrentPage,
            "El ticket se canceló correctamente, " +
            "pero no fue posible actualizar el listado.");
    }
}