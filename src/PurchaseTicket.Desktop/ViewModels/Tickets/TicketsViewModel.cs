using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Application.UseCases.PurchaseTickets.GetFilterOptions;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Pantalla Tickets: todos los tickets, del mas reciente al mas antiguo, con
// busqueda y filtros. Cada filtro recarga la lista al cambiar, desde la pagina 1.
// Los filtros se pliegan; cerrados, los activos se ven como etiquetas
public class TicketsViewModel : PagedListViewModel<TicketListItem>
{
    // Campos

    private static readonly FilterOption<int> AllSuppliers = new(null, "Todos");
    private static readonly FilterOption<int> AllMaterials = new(null, "Todos");

    private readonly ITicketsDialogService _dialogs;
    private readonly TimeProvider _timeProvider;

    private FilterOption<TicketPeriod> _selectedPeriod;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private FilterOption<int> _selectedSupplier = AllSuppliers;
    private FilterOption<int> _selectedMaterial = AllMaterials;
    private FilterOption<TicketStatus> _selectedStatus;
    private FilterOption<WeighingType> _selectedWeighingType;
    private string _dateRangeError = string.Empty;
    private IReadOnlyList<FilterOption<int>> _supplierOptions = [AllSuppliers];
    private IReadOnlyList<FilterOption<int>> _materialOptions = [AllMaterials];
    private IReadOnlyList<ActiveFilter> _activeFilters = [];

    // Cerrado al abrir la aplicacion; la pantalla vive mientras la app esta
    // abierta, asi que recuerda como lo dejo el operador
    private bool _isFiltersExpanded;

    // Evita recargar mientras se cambian varios filtros a la vez desde el codigo
    private bool _isUpdatingFilters;

    // Constructor

    public TicketsViewModel(
        IServiceScopeFactory scopeFactory,
        ITicketsDialogService dialogs,
        TimeProvider timeProvider,
        ILogger<TicketsViewModel> logger)
        : base(scopeFactory, logger)
    {
        _dialogs = dialogs;
        _timeProvider = timeProvider;

        _selectedPeriod = PeriodOptions[0];
        _selectedStatus = StatusOptions[0];
        _selectedWeighingType = WeighingTypeOptions[0];

        ClearFiltersCommand = new AsyncRelayCommand(
            ClearFiltersAsync,
            CanInteract);

        ToggleFiltersCommand = new RelayCommand(
            () => IsFiltersExpanded = !IsFiltersExpanded);

        RemoveFilterCommand = new AsyncRelayCommand<ActiveFilter>(
            RemoveFilterAsync,
            _ => CanInteract());

        OpenDetailCommand = new AsyncRelayCommand<TicketListItem>(
            OpenDetailAsync,
            _ => CanInteract());
    }

    // Propiedades

    public string Description =>
        "Todos los tickets, del más reciente al más antiguo. " +
        "Abre uno para corregirlo, reimprimirlo o cancelarlo";

    public bool IsFiltersExpanded
    {
        get => _isFiltersExpanded;
        set
        {
            if (SetProperty(ref _isFiltersExpanded, value))
                OnPropertyChanged(nameof(ShowsActiveFilters));
        }
    }

    // Filtros distintos de "Todos", en el orden del panel
    public IReadOnlyList<ActiveFilter> ActiveFilters
    {
        get => _activeFilters;
        private set
        {
            if (!SetProperty(ref _activeFilters, value))
                return;

            OnPropertyChanged(nameof(ActiveFilterCount));
            OnPropertyChanged(nameof(HasActiveFilters));
            OnPropertyChanged(nameof(ShowsActiveFilters));
        }
    }

    public int ActiveFilterCount => ActiveFilters.Count;

    public bool HasActiveFilters => ActiveFilters.Count > 0;

    // Con el panel abierto los filtros ya estan a la vista
    public bool ShowsActiveFilters => HasActiveFilters && !IsFiltersExpanded;

    public IReadOnlyList<FilterOption<TicketPeriod>> PeriodOptions { get; } =
    [
        new(null, "Todos"),
        new(TicketPeriod.Today, "Hoy"),
        new(TicketPeriod.Yesterday, "Ayer"),
        new(TicketPeriod.Last7Days, "Últimos 7 días"),
        new(TicketPeriod.ThisMonth, "Este mes"),
        new(TicketPeriod.LastMonth, "Mes anterior"),
        new(TicketPeriod.Custom, "Personalizado")
    ];

    // Se reemplazan completas, no se vacian y llenan: al vaciar una lista el
    // selector pierde su seleccion y no siempre la recupera. Con una lista
    // nueva conserva la opcion elegida si encuentra una igual (es un record)
    public IReadOnlyList<FilterOption<int>> SupplierOptions
    {
        get => _supplierOptions;
        private set => SetProperty(ref _supplierOptions, value);
    }

    public IReadOnlyList<FilterOption<int>> MaterialOptions
    {
        get => _materialOptions;
        private set => SetProperty(ref _materialOptions, value);
    }

    public IReadOnlyList<FilterOption<TicketStatus>> StatusOptions { get; } =
    [
        new(null, "Todos"),
        new(TicketStatus.WeighingPending, "Pendiente de pesaje"),
        new(TicketStatus.AmountPending, "Pendiente de importe"),
        new(TicketStatus.Completed, "Completado"),
        new(TicketStatus.Cancelled, "Cancelado")
    ];

    public IReadOnlyList<FilterOption<WeighingType>> WeighingTypeOptions { get; } =
    [
        new(null, "Todas"),
        new(WeighingType.Conventional, "Convencional"),
        new(WeighingType.Single, "Un solo pesaje")
    ];

    public FilterOption<TicketPeriod> SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            SetFilter(ref _selectedPeriod, value ?? PeriodOptions[0]);
            OnPropertyChanged(nameof(IsCustomPeriod));
        }
    }

    // Desde y Hasta solo se muestran y se usan con un periodo personalizado
    public bool IsCustomPeriod => SelectedPeriod.Value == TicketPeriod.Custom;

    // Fechas locales del ultimo pesaje; las dos incluyen el dia completo
    public DateTime? FromDate
    {
        get => _fromDate;
        set => SetFilter(ref _fromDate, value);
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set => SetFilter(ref _toDate, value);
    }

    public FilterOption<int> SelectedSupplier
    {
        get => _selectedSupplier;
        set => SetFilter(ref _selectedSupplier, value ?? AllSuppliers);
    }

    public FilterOption<int> SelectedMaterial
    {
        get => _selectedMaterial;
        set => SetFilter(ref _selectedMaterial, value ?? AllMaterials);
    }

    public FilterOption<TicketStatus> SelectedStatus
    {
        get => _selectedStatus;
        set => SetFilter(ref _selectedStatus, value ?? StatusOptions[0]);
    }

    public FilterOption<WeighingType> SelectedWeighingType
    {
        get => _selectedWeighingType;
        set => SetFilter(ref _selectedWeighingType, value ?? WeighingTypeOptions[0]);
    }

    public string DateRangeError
    {
        get => _dateRangeError;
        private set
        {
            if (SetProperty(ref _dateRangeError, value))
                OnPropertyChanged(nameof(HasDateRangeError));
        }
    }

    public bool HasDateRangeError => !string.IsNullOrEmpty(DateRangeError);

    // Comandos

    public AsyncRelayCommand ClearFiltersCommand { get; }

    public RelayCommand ToggleFiltersCommand { get; }

    // La × de una etiqueta
    public AsyncRelayCommand<ActiveFilter> RemoveFilterCommand { get; }

    // Boton "Ver detalle" o doble clic en la fila
    public AsyncRelayCommand<TicketListItem> OpenDetailCommand { get; }

    // Propiedades protegidas

    protected override bool HasExtraFilters => HasActiveFilters;

    protected override string LoadErrorMessage =>
        "No fue posible cargar los tickets.";

    protected override string EmptyListMessage =>
        "Todavía no hay tickets registrados.";

    protected override string NoResultsMessage =>
        "No se encontraron tickets con los filtros aplicados.";

    // Metodos publicos

    // Al entrar a la pantalla: las opciones pueden haber cambiado desde la
    // ultima vez, por ejemplo un ticket con un proveedor nuevo
    public async Task InitializeAsync()
    {
        await LoadFilterOptionsAsync();
        await LoadAsync();
    }

    // Metodos protegidos

    protected override async Task<PagedResult<TicketListItem>> FetchPageAsync(
        IServiceProvider services,
        int page,
        int pageSize,
        string? search)
    {
        var (fromUtc, toUtc) = GetDateRangeUtc();

        var result = await services
            .GetRequiredService<GetPurchaseTickets>()
            .ExecuteAsync(new GetPurchaseTicketsQuery(
                Page: page,
                PageSize: pageSize,
                Search: search,
                Status: SelectedStatus.Value,
                WeighingType: SelectedWeighingType.Value,
                WeighedFrom: fromUtc,
                WeighedTo: toUtc,
                SupplierId: SelectedSupplier.Value,
                MaterialId: SelectedMaterial.Value));

        var items = result.Items
            .Select(ticket => TicketListItem.From(ticket, _timeProvider))
            .ToList();

        return new PagedResult<TicketListItem>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    protected override void NotifyCommandsCanExecuteChanged()
    {
        base.NotifyCommandsCanExecuteChanged();

        ClearFiltersCommand?.NotifyCanExecuteChanged();
        RemoveFilterCommand?.NotifyCanExecuteChanged();
        OpenDetailCommand?.NotifyCanExecuteChanged();
    }

    // Metodos privados

    // El DatePicker entrega la fecha sin zona; se marca asi para convertirla a UTC
    private static DateTime LocalDay(DateTime value)
    {
        return DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
    }

    private static string FormatDate(DateTime value)
    {
        return value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
    }

    private static FilterOption<int> ToOption(CatalogOption option)
    {
        // Un inactivo sigue en la lista porque tiene tickets
        return new FilterOption<int>(
            option.Id,
            option.IsActive ? option.Name : $"{option.Name} (inactivo)");
    }

    private static IReadOnlyList<FilterOption<int>> BuildOptions(
        FilterOption<int> all,
        IReadOnlyList<CatalogOption> options)
    {
        return [all, .. options.Select(ToOption)];
    }

    // Comun a todos los filtros. El nombre llega por CallerMemberName: los
    // filtros comparten tipo (dos fechas, proveedor y material) y no se distinguen por el
    private void SetFilter<T>(
        ref T field,
        T value,
        [CallerMemberName] string propertyName = "")
    {
        if (!SetProperty(ref field, value, propertyName))
            return;

        RefreshActiveFilters();

        if (_isUpdatingFilters)
            return;

        // Con un rango invertido no se consulta: se muestra el aviso
        if (!ValidateDateRange())
            return;

        SearchCommand.Execute(null);
    }

    private void RefreshActiveFilters()
    {
        var filters = new List<ActiveFilter>();

        string? period = DescribePeriod();

        if (period is not null)
            filters.Add(new ActiveFilter(ActiveFilterKind.Period, "Periodo", period));

        if (SelectedSupplier.Value is not null)
            filters.Add(new ActiveFilter(ActiveFilterKind.Supplier, "Proveedor", SelectedSupplier.Label));

        if (SelectedMaterial.Value is not null)
            filters.Add(new ActiveFilter(ActiveFilterKind.Material, "Material", SelectedMaterial.Label));

        if (SelectedStatus.Value is not null)
            filters.Add(new ActiveFilter(ActiveFilterKind.Status, "Estado", SelectedStatus.Label));

        if (SelectedWeighingType.Value is not null)
            filters.Add(new ActiveFilter(ActiveFilterKind.WeighingType, "Modalidad", SelectedWeighingType.Label));

        ActiveFilters = filters;
    }

    // Null si el periodo no filtra: "Todos", o personalizado sin fechas
    private string? DescribePeriod()
    {
        if (!IsCustomPeriod)
            return SelectedPeriod.Value is null ? null : SelectedPeriod.Label;

        return (FromDate, ToDate) switch
        {
            (DateTime from, DateTime to) => $"{FormatDate(from)} – {FormatDate(to)}",
            (DateTime from, null) => $"desde {FormatDate(from)}",
            (null, DateTime to) => $"hasta {FormatDate(to)}",
            _ => null
        };
    }

    private bool ValidateDateRange()
    {
        bool isValid =
            !IsCustomPeriod
            || FromDate is null
            || ToDate is null
            || FromDate.Value.Date <= ToDate.Value.Date;

        DateRangeError = isValid
            ? string.Empty
            : "La fecha inicial no puede ser posterior a la final.";

        return isValid;
    }

    // Dias completos en hora local, convertidos a UTC como los guarda la base.
    // El fin es el inicio del dia siguiente, que la consulta no incluye
    private (DateTime? FromUtc, DateTime? ToUtc) GetDateRangeUtc()
    {
        TimeZoneInfo zone = _timeProvider.LocalTimeZone;

        DateTime today = TimeZoneInfo
            .ConvertTime(_timeProvider.GetUtcNow(), zone)
            .Date;

        DateTime firstOfMonth = new(today.Year, today.Month, 1);

        (DateTime? Start, DateTime? End) range = SelectedPeriod.Value switch
        {
            TicketPeriod.Today => (today, today.AddDays(1)),
            TicketPeriod.Yesterday => (today.AddDays(-1), today),
            TicketPeriod.Last7Days => (today.AddDays(-6), today.AddDays(1)),
            TicketPeriod.ThisMonth => (firstOfMonth, today.AddDays(1)),
            TicketPeriod.LastMonth => (firstOfMonth.AddMonths(-1), firstOfMonth),
            TicketPeriod.Custom => (
                FromDate is DateTime from ? LocalDay(from) : null,
                ToDate is DateTime to ? LocalDay(to).AddDays(1) : null),
            _ => (null, null)
        };

        return (
            range.Start is DateTime start ? TimeZoneInfo.ConvertTimeToUtc(start, zone) : null,
            range.End is DateTime end ? TimeZoneInfo.ConvertTimeToUtc(end, zone) : null);
    }

    private async Task LoadFilterOptionsAsync()
    {
        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();

            var options = await scope.ServiceProvider
                .GetRequiredService<GetTicketFilterOptions>()
                .ExecuteAsync();

            var suppliers = BuildOptions(AllSuppliers, options.Suppliers);
            var materials = BuildOptions(AllMaterials, options.Materials);

            int? supplierId = SelectedSupplier.Value;
            int? materialId = SelectedMaterial.Value;

            _isUpdatingFilters = true;

            try
            {
                SupplierOptions = suppliers;
                MaterialOptions = materials;

                // Se conserva lo elegido si sigue existiendo; si no, vuelve a "Todos"
                SelectedSupplier = suppliers.FirstOrDefault(o => o.Value == supplierId) ?? AllSuppliers;
                SelectedMaterial = materials.FirstOrDefault(o => o.Value == materialId) ?? AllMaterials;
            }
            finally
            {
                _isUpdatingFilters = false;
            }

            // Si la opcion elegida no cambio, SetProperty no avisa; se avisa
            // igual para que el selector la muestre sobre la lista nueva
            OnPropertyChanged(nameof(SelectedSupplier));
            OnPropertyChanged(nameof(SelectedMaterial));

            RefreshActiveFilters();
        }
        catch (Exception ex)
        {
            // Sin opciones la lista sigue funcionando, solo sin esos dos filtros
            Logger.LogError(ex, "No fue posible cargar las opciones de los filtros de tickets.");
        }
    }

    private async Task OpenDetailAsync(TicketListItem ticket)
    {
        if (!_dialogs.ShowDetailDialog(ticket.Id))
            return;

        // Por ejemplo, se cancelo desde el detalle: la fila debe mostrarlo
        await ReloadAfterActionAsync(
            CurrentPage,
            "No fue posible actualizar el listado.");
    }

    // Quitar una etiqueta es elegir "Todos" en ese filtro: el setter recarga
    private Task RemoveFilterAsync(ActiveFilter filter)
    {
        switch (filter.Kind)
        {
            case ActiveFilterKind.Period:
                // Las fechas se limpian sin recargar; recarga el cambio de periodo
                _isUpdatingFilters = true;

                try
                {
                    FromDate = null;
                    ToDate = null;
                }
                finally
                {
                    _isUpdatingFilters = false;
                }

                SelectedPeriod = PeriodOptions[0];
                break;

            case ActiveFilterKind.Supplier:
                SelectedSupplier = AllSuppliers;
                break;

            case ActiveFilterKind.Material:
                SelectedMaterial = AllMaterials;
                break;

            case ActiveFilterKind.Status:
                SelectedStatus = StatusOptions[0];
                break;

            case ActiveFilterKind.WeighingType:
                SelectedWeighingType = WeighingTypeOptions[0];
                break;
        }

        return Task.CompletedTask;
    }

    private async Task ClearFiltersAsync()
    {
        _isUpdatingFilters = true;

        try
        {
            SearchText = string.Empty;
            SelectedPeriod = PeriodOptions[0];
            FromDate = null;
            ToDate = null;
            SelectedSupplier = AllSuppliers;
            SelectedMaterial = AllMaterials;
            SelectedStatus = StatusOptions[0];
            SelectedWeighingType = WeighingTypeOptions[0];
        }
        finally
        {
            _isUpdatingFilters = false;
        }

        ValidateDateRange();

        await LoadPageAsync(1);
    }
}