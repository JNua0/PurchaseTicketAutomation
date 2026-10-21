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
    private readonly TimeProvider _timeProvider;

    private WeighingView _selectedView = WeighingView.InPlant;
    private int _inPlantCount;

    // Constructor

    public VehicleWeighingViewModel(
        IServiceScopeFactory scopeFactory,
        IWeighingDialogService weighingDialogService,
        TimeProvider timeProvider,
        ILogger<VehicleWeighingViewModel> logger)
        : base(scopeFactory, logger)
    {
        _weighingDialogService = weighingDialogService;
        _timeProvider = timeProvider;

        NewWeighingCommand = new AsyncRelayCommand(
            NewWeighingAsync,
            CanInteract);
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

    protected override string LoadErrorMessage =>
        "No fue posible cargar los pesajes.";

    protected override string EmptyListMessage =>
        IsInPlantView
            ? "No hay vehículos en planta."
            : "Hoy no hay pesajes únicos pendientes de importe.";

    protected override string NoResultsMessage =>
        "No se encontraron pesajes con la búsqueda aplicada.";

    // Comandos

    public AsyncRelayCommand NewWeighingCommand { get; }

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

    private async Task NewWeighingAsync()
    {
        if (!_weighingDialogService.ShowCreateDialog())
            return;

        await ReloadAfterActionAsync(
            page: 1,
            "El pesaje se registró correctamente, " +
            "pero no fue posible actualizar el listado.");
    }
}