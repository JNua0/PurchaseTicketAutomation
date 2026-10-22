using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.Amounts;

// Pantalla Importes: todos los tickets pendientes de importe, sin filtro de
// fechas, del que lleva mas tiempo esperando al mas reciente
public class TicketAmountsViewModel : PagedListViewModel<AmountListItem>
{
    // Campos

    private readonly IAmountDialogService _amountDialogService;
    private readonly INotificationService _notifications;
    private readonly ITicketActionsService _ticketActions;
    private readonly TimeProvider _timeProvider;
    private int _pendingCount;

    // Constructor

    public TicketAmountsViewModel(
        IServiceScopeFactory scopeFactory,
        IAmountDialogService amountDialogService,
        INotificationService notifications,
        ITicketActionsService ticketActions,
        TimeProvider timeProvider,
        ILogger<TicketAmountsViewModel> logger)
        : base(scopeFactory, logger)
    {
        _amountDialogService = amountDialogService;
        _notifications = notifications;
        _ticketActions = ticketActions;
        _timeProvider = timeProvider;

        RegisterAmountCommand = new AsyncRelayCommand<AmountListItem>(
            RegisterAmountAsync,
            CanActOn);

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

    public AsyncRelayCommand<AmountListItem> RegisterAmountCommand { get; }

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

        RegisterAmountCommand?.NotifyCanExecuteChanged();
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

    private async Task RegisterAmountAsync(AmountListItem ticket)
    {
        var registration =
            _amountDialogService.ShowRegisterAmountDialog(ticket);

        if (registration is null)
            return;

        _notifications.ShowSuccess(
            $"Importe registrado en el folio {registration.TicketNumber}: " +
            $"${registration.Amount:N2}. El ticket quedó completado.");

        // El ticket sale de la lista: ya no esta pendiente de importe
        await ReloadAfterActionAsync(
            CurrentPage,
            "El importe se registró correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task CancelTicketAsync(AmountListItem ticket)
    {
        CancelTicketOutcome outcome;

        IsBusy = true;

        try
        {
            outcome = await _ticketActions.CancelAsync(ticket.Id, ticket.TicketNumber);
        }
        finally
        {
            IsBusy = false;
        }

        await ReloadAfterCancelAsync(outcome);
    }
}