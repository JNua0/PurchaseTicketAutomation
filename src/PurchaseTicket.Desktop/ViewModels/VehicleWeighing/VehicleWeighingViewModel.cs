using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Pantalla de pesaje con dos vistas que no se repiten: los convencionales
// en planta y los pesajes unicos de hoy. Busqueda y paginacion vienen de la base
public class VehicleWeighingViewModel : PagedListViewModel<WeighingListItem>
{
    // Campos

    private readonly IWeighingDialogService _weighingDialogService;
    private readonly INotificationService _notifications;
    private readonly ITicketActionsService _ticketActions;
    private readonly TimeProvider _timeProvider;
    private WeighingView _selectedView = WeighingView.InPlant;
    private int _inPlantCount;

    // Constructor

    public VehicleWeighingViewModel(
        IServiceScopeFactory scopeFactory,
        IWeighingDialogService weighingDialogService,
        INotificationService notifications,
        ITicketActionsService ticketActions,
        TimeProvider timeProvider,
        ILogger<VehicleWeighingViewModel> logger)
        : base(scopeFactory, logger)
    {
        _weighingDialogService = weighingDialogService;
        _notifications = notifications;
        _ticketActions = ticketActions;
        _timeProvider = timeProvider;

        NewWeighingCommand = new AsyncRelayCommand(
            NewWeighingAsync,
            CanInteract);

        RegisterTareCommand = new AsyncRelayCommand<WeighingListItem>(
            RegisterTareAsync,
            CanRegisterTare);

        EditTicketCommand = new AsyncRelayCommand<WeighingListItem>(
            EditTicketAsync,
            CanActOn);

        ReprintCommand = new AsyncRelayCommand<WeighingListItem>(
            ReprintAsync,
            CanActOn);

        CancelTicketCommand = new AsyncRelayCommand<WeighingListItem>(
            CancelTicketAsync,
            CanActOn);
    }

    // Propiedades

    public IReadOnlyList<WeighingViewOption> ViewOptions { get; } =
    [
        new(WeighingView.InPlant, "En planta", ShowsCount: true),
        new(WeighingView.SinglesToday, "Únicos de hoy", ShowsCount: false)
    ];

    public WeighingView SelectedView
    {
        get => _selectedView;
        set
        {
            if (!SetProperty(ref _selectedView, value))
                return;

            OnPropertyChanged(nameof(IsInPlantView));
            OnPropertyChanged(nameof(ViewDescription));

            // Cambiar de vista es una accion deliberada: recargar desde la primera pagina
            SearchCommand.Execute(null);
        }
    }

    public bool IsInPlantView => SelectedView == WeighingView.InPlant;

    public string ViewDescription =>
        IsInPlantView
            ? "Vehículos en planta que esperan su pesaje de salida"
            : "Pesajes únicos de hoy, pendientes de importe";

    // Total de vehiculos en planta, sin importar la vista ni la busqueda
    public int InPlantCount
    {
        get => _inPlantCount;
        private set => SetProperty(ref _inPlantCount, value);
    }

    // Comandos

    public AsyncRelayCommand NewWeighingCommand { get; }

    public AsyncRelayCommand<WeighingListItem> RegisterTareCommand { get; }

    public AsyncRelayCommand<WeighingListItem> EditTicketCommand { get; }

    public AsyncRelayCommand<WeighingListItem> ReprintCommand { get; }

    public AsyncRelayCommand<WeighingListItem> CancelTicketCommand { get; }

    // Propiedades protegidas

    protected override string LoadErrorMessage =>
        "No fue posible cargar los pesajes.";

    protected override string EmptyListMessage =>
        IsInPlantView
            ? "No hay vehículos en planta."
            : "Hoy no hay pesajes únicos pendientes de importe.";

    protected override string NoResultsMessage =>
        "No se encontraron pesajes con la búsqueda aplicada.";

    // Metodos protegidos

    protected override async Task<PagedResult<WeighingListItem>> FetchPageAsync(
        IServiceProvider services,
        int page,
        int pageSize,
        string? search)
    {
        var getTickets =
            services.GetRequiredService<GetPurchaseTickets>();

        var result = await getTickets.ExecuteAsync(
            BuildQuery(page, pageSize, search));

        // Sin busqueda, la vista en planta ya trae el total; en otro
        // caso se cuenta aparte pidiendo una sola fila
        if (IsInPlantView && search is null)
        {
            InPlantCount = result.TotalCount;
        }
        else
        {
            var inPlant = await getTickets.ExecuteAsync(
                InPlantQuery(page: 1, pageSize: 1, search: null));

            InPlantCount = inPlant.TotalCount;
        }

        var items = result.Items
            .Select(ticket => WeighingListItem.From(ticket, _timeProvider))
            .ToList();

        return new PagedResult<WeighingListItem>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    protected override void NotifyCommandsCanExecuteChanged()
    {
        base.NotifyCommandsCanExecuteChanged();

        NewWeighingCommand?.NotifyCanExecuteChanged();
        RegisterTareCommand?.NotifyCanExecuteChanged();
        EditTicketCommand?.NotifyCanExecuteChanged();
        ReprintCommand?.NotifyCanExecuteChanged();
        CancelTicketCommand?.NotifyCanExecuteChanged();
    }

    // Metodos privados

    private static GetPurchaseTicketsQuery InPlantQuery(
        int page,
        int pageSize,
        string? search)
    {
        return new GetPurchaseTicketsQuery(
            Page: page,
            PageSize: pageSize,
            Search: search,
            Status: TicketStatus.WeighingPending,
            WeighingType: WeighingType.Conventional);
    }

    private GetPurchaseTicketsQuery BuildQuery(
        int page,
        int pageSize,
        string? search)
    {
        if (IsInPlantView)
            return InPlantQuery(page, pageSize, search);

        var (startUtc, endUtc) = GetTodayRangeUtc();

        return new GetPurchaseTicketsQuery(
            Page: page,
            PageSize: pageSize,
            Search: search,
            Status: TicketStatus.AmountPending,
            WeighingType: WeighingType.Single,
            WeighedFrom: startUtc,
            WeighedTo: endUtc);
    }

    // "Hoy" se decide con la hora local de la planta, pero la base guarda UTC:
    // se toman las 00:00 de hoy y de manana en hora local y se pasan a UTC
    private (DateTime StartUtc, DateTime EndUtc) GetTodayRangeUtc()
    {
        TimeZoneInfo zone = _timeProvider.LocalTimeZone;

        DateTime today = TimeZoneInfo
            .ConvertTime(_timeProvider.GetUtcNow(), zone)
            .Date;

        return (
            TimeZoneInfo.ConvertTimeToUtc(today, zone),
            TimeZoneInfo.ConvertTimeToUtc(today.AddDays(1), zone));
    }

    // Las dos vistas solo muestran tickets pendientes: todos se pueden
    // reimprimir y cancelar. El dominio vuelve a revisar el estado al guardar
    private bool CanActOn(WeighingListItem ticket)
    {
        return ticket is not null && CanInteract();
    }

    private async Task EditTicketAsync(WeighingListItem ticket)
    {
        if (!_weighingDialogService.ShowEditDialog(ticket))
            return;

        _notifications.ShowSuccess(
            $"Ticket folio {ticket.TicketNumber} corregido.");

        await ReloadAfterActionAsync(
            CurrentPage,
            "El ticket se actualizó correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task ReprintAsync(WeighingListItem ticket)
    {
        // Ocupado mientras imprime: la impresora atiende un ticket a la vez
        IsBusy = true;

        try
        {
            await _ticketActions.ReprintAsync(ticket.Id, ticket.TicketNumber);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelTicketAsync(WeighingListItem ticket)
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

    private bool CanRegisterTare(WeighingListItem ticket)
    {
        return ticket is not null
            && ticket.CanRegisterTare
            && CanInteract();
    }

    private async Task RegisterTareAsync(WeighingListItem ticket)
    {
        var registration =
            _weighingDialogService.ShowRegisterTareDialog(ticket);

        if (registration is null)
            return;

        _notifications.ShowSuccess(
            $"Tara registrada en el folio {registration.TicketNumber}: " +
            $"peso neto {registration.NetWeight:N0} kg.");

        if (registration.PrintWarning is not null)
        {
            _notifications.ShowWarning(
                $"No se imprimió el folio {registration.TicketNumber}. " +
                registration.PrintWarning);
        }

        // El ticket sale de "En planta": pasa a pendiente de importe
        await ReloadAfterActionAsync(
            CurrentPage,
            "La tara se registró correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task NewWeighingAsync()
    {
        var registration = _weighingDialogService.ShowCreateDialog();

        if (registration is null)
            return;

        _notifications.ShowSuccess(
            $"Pesaje registrado en el folio {registration.TicketNumber}.");

        // El registro si se hizo: el problema de impresion va en un aviso
        // aparte que espera a que el operador lo lea y lo cierre
        if (registration.PrintWarning is not null)
        {
            _notifications.ShowWarning(
                $"No se imprimió el folio {registration.TicketNumber}. " +
                registration.PrintWarning);
        }

        // Se muestra la vista donde quedo el ticket nuevo
        WeighingView targetView =
            registration.WeighingType == WeighingType.Conventional
                ? WeighingView.InPlant
                : WeighingView.SinglesToday;

        // Cambiar de vista ya recarga la lista; recargar otra vez se cruzaria
        if (SelectedView != targetView)
        {
            SelectedView = targetView;
            return;
        }

        await ReloadAfterActionAsync(
            page: 1,
            "El pesaje se registró correctamente, " +
            "pero no fue posible actualizar el listado.");
    }
}