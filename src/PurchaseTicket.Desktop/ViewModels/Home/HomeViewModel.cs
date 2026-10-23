using System.Globalization;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Devices;
using PurchaseTicket.Application.UseCases.Home.GetSummary;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Amounts;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.ViewModels.Home;

// Pantalla Inicio: el resumen del dia y lo que espera atencion. Se actualiza
// al entrar, despues de registrar algo desde aqui y cada minuto mientras esta
// abierta, para que el tiempo en planta avance solo
public sealed class HomeViewModel : ObservableObject
{
    // Campos

    // Un vehiculo con mas de este tiempo en planta se resalta en ambar
    private static readonly TimeSpan LongStay = TimeSpan.FromHours(2);

    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-MX");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWeighingDialogService _weighingDialogs;
    private readonly IAmountDialogService _amountDialogs;
    private readonly INotificationService _notifications;
    private readonly ApplicationSettingsService _settings;
    private readonly ISerialPortDiscovery _portDiscovery;
    private readonly TimeProvider _clock;
    private readonly ILogger<HomeViewModel> _logger;
    private readonly DispatcherTimer _timer;

    private IReadOnlyList<HomeInPlantRow> _inPlant = [];
    private IReadOnlyList<HomeAmountRow> _amountPending = [];
    private int _inPlantCount;
    private int _amountPendingCount;
    private string _inPlantNote = string.Empty;
    private string _amountPendingNote = string.Empty;
    private string _todayNetWeightText = "0";
    private int _todayTicketCount;
    private string _todayBreakdown = string.Empty;
    private string _description = string.Empty;
    private string _errorMessage = string.Empty;
    private HomeDeviceStatus _indicator = new("Indicador · revisando…", StatusKind.Neutral);
    private HomeDeviceStatus _printer = new("Impresora · revisando…", StatusKind.Neutral);
    private bool _isRefreshing;

    // Mientras hay una ventana abierta desde aqui, el temporizador no recarga
    private bool _isDialogOpen;

    // Constructor

    public HomeViewModel(
        IServiceScopeFactory scopeFactory,
        IWeighingDialogService weighingDialogs,
        IAmountDialogService amountDialogs,
        INotificationService notifications,
        ApplicationSettingsService settings,
        ISerialPortDiscovery portDiscovery,
        TimeProvider clock,
        ILogger<HomeViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _weighingDialogs = weighingDialogs;
        _amountDialogs = amountDialogs;
        _notifications = notifications;
        _settings = settings;
        _portDiscovery = portDiscovery;
        _clock = clock;
        _logger = logger;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _timer.Tick += OnTimerTick;

        NewWeighingCommand = new AsyncRelayCommand(NewWeighingAsync, () => !_isDialogOpen);

        RegisterTareCommand = new AsyncRelayCommand<HomeInPlantRow>(
            RegisterTareAsync,
            row => row is not null && !_isDialogOpen);

        RegisterAmountCommand = new AsyncRelayCommand<HomeAmountRow>(
            RegisterAmountAsync,
            row => row is not null && !_isDialogOpen);
    }

    // Propiedades

    // "Jueves 22 de octubre de 2026 · actualizado a las 22:25"
    public string Description
    {
        get => _description;
        private set => SetProperty(ref _description, value);
    }

    public HomeDeviceStatus Indicator
    {
        get => _indicator;
        private set => SetProperty(ref _indicator, value);
    }

    public HomeDeviceStatus Printer
    {
        get => _printer;
        private set => SetProperty(ref _printer, value);
    }

    public int InPlantCount
    {
        get => _inPlantCount;
        private set
        {
            if (!SetProperty(ref _inPlantCount, value))
                return;

            OnPropertyChanged(nameof(HasInPlant));
            OnPropertyChanged(nameof(IsInPlantEmpty));
            OnPropertyChanged(nameof(InPlantUnit));
        }
    }

    public bool HasInPlant => InPlantCount > 0;

    public bool IsInPlantEmpty => !HasInPlant;

    public string InPlantUnit => InPlantCount == 1 ? "vehículo" : "vehículos";

    public string InPlantNote
    {
        get => _inPlantNote;
        private set => SetProperty(ref _inPlantNote, value);
    }

    public int AmountPendingCount
    {
        get => _amountPendingCount;
        private set
        {
            if (!SetProperty(ref _amountPendingCount, value))
                return;

            OnPropertyChanged(nameof(HasAmountPending));
            OnPropertyChanged(nameof(IsAmountPendingEmpty));
            OnPropertyChanged(nameof(AmountPendingUnit));
        }
    }

    public bool HasAmountPending => AmountPendingCount > 0;

    public bool IsAmountPendingEmpty => !HasAmountPending;

    public string AmountPendingUnit => AmountPendingCount == 1 ? "ticket" : "tickets";

    public string AmountPendingNote
    {
        get => _amountPendingNote;
        private set => SetProperty(ref _amountPendingNote, value);
    }

    public string TodayNetWeightText
    {
        get => _todayNetWeightText;
        private set => SetProperty(ref _todayNetWeightText, value);
    }

    public int TodayTicketCount
    {
        get => _todayTicketCount;
        private set
        {
            if (SetProperty(ref _todayTicketCount, value))
                OnPropertyChanged(nameof(TodayTicketUnit));
        }
    }

    public string TodayTicketUnit => TodayTicketCount == 1 ? "ticket" : "tickets";

    // "5 convencionales · 3 únicos"
    public string TodayBreakdown
    {
        get => _todayBreakdown;
        private set => SetProperty(ref _todayBreakdown, value);
    }

    // Los mas antiguos primero, hasta GetHomeSummary.ListSize
    public IReadOnlyList<HomeInPlantRow> InPlant
    {
        get => _inPlant;
        private set => SetProperty(ref _inPlant, value);
    }

    public IReadOnlyList<HomeAmountRow> AmountPending
    {
        get => _amountPending;
        private set => SetProperty(ref _amountPending, value);
    }

    // "3" o "8 · se muestran los 5 más antiguos"
    public string InPlantListCount => ListCount(InPlantCount);

    public string AmountPendingListCount => ListCount(AmountPendingCount);

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    // Comandos

    public AsyncRelayCommand NewWeighingCommand { get; }

    public AsyncRelayCommand<HomeInPlantRow> RegisterTareCommand { get; }

    public AsyncRelayCommand<HomeAmountRow> RegisterAmountCommand { get; }

    // Metodos publicos

    // La vista lo llama al mostrarse: cada vez que se entra a Inicio
    public async Task ActivateAsync()
    {
        _timer.Start();

        await RefreshAsync();
    }

    // La vista lo llama al cambiar de pantalla: sin Inicio visible no se recarga
    public void Deactivate()
    {
        _timer.Stop();
    }

    // Metodos privados

    private static string ListCount(int count)
    {
        return count > GetHomeSummary.ListSize
            ? $"{count} · se muestran los {GetHomeSummary.ListSize} más antiguos"
            : count.ToString(CultureInfo.CurrentCulture);
    }

    private static string Plural(int count, string singular, string plural)
    {
        return $"{count} {(count == 1 ? singular : plural)}";
    }

    // Npgsql ya devuelve UTC; esto solo protege de un DateTime sin tipo
    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        if (_isDialogOpen)
            return;

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        // Un clic y el temporizador pueden coincidir: basta con una carga
        if (_isRefreshing)
            return;

        _isRefreshing = true;

        try
        {
            await LoadSummaryAsync();
            await LoadDevicesAsync();
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private async Task LoadSummaryAsync()
    {
        TimeZoneInfo zone = _clock.LocalTimeZone;
        DateTime nowLocal = TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), zone).DateTime;
        DateTime todayLocal = nowLocal.Date;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            // El dia local de la PC, en UTC como se guarda en la base
            var summary = await scope.ServiceProvider
                .GetRequiredService<GetHomeSummary>()
                .ExecuteAsync(new GetHomeSummaryQuery(
                    TimeZoneInfo.ConvertTimeToUtc(todayLocal, zone),
                    TimeZoneInfo.ConvertTimeToUtc(todayLocal.AddDays(1), zone)));

            Apply(summary, todayLocal);

            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            // Se conservan los ultimos datos; el mensaje avisa que pueden estar viejos
            ErrorMessage = "No fue posible actualizar el resumen. Se intentará de nuevo en un minuto.";

            _logger.LogError(ex, "No fue posible cargar el resumen de Inicio.");
        }

        string date = nowLocal.ToString("dddd d 'de' MMMM 'de' yyyy", Spanish);

        Description = $"{char.ToUpper(date[0], Spanish)}{date[1..]} · actualizado a las {nowLocal:HH:mm}";
    }

    private void Apply(HomeSummary summary, DateTime todayLocal)
    {
        DateTime nowUtc = _clock.GetUtcNow().UtcDateTime;

        InPlant = summary.InPlant
            .Select(ticket =>
            {
                TimeSpan elapsed = nowUtc - AsUtc(ticket.CheckInAt);

                return new HomeInPlantRow(
                    WeighingListItem.From(ticket, _clock),
                    WeighingListItem.FormatElapsed(elapsed),
                    elapsed >= LongStay);
            })
            .ToList();

        AmountPending = summary.AmountPending
            .Select(ticket =>
            {
                var item = AmountListItem.From(ticket, _clock);

                return new HomeAmountRow(item, item.WeighedAt.Date < todayLocal);
            })
            .ToList();

        InPlantCount = summary.InPlantCount;
        AmountPendingCount = summary.AmountPendingCount;

        OnPropertyChanged(nameof(InPlantListCount));
        OnPropertyChanged(nameof(AmountPendingListCount));

        // La lista ya viene del mas antiguo al mas reciente
        InPlantNote = InPlant.Count > 0
            ? $"El más antiguo lleva {InPlant[0].Elapsed}"
            : "Ningún vehículo espera su tara";

        AmountPendingNote = AmountPending.Count > 0
            ? $"El más antiguo es del {AmountPending[0].WeighedDate}"
            : "No hay tickets por registrar";

        var today = summary.Today;

        TodayNetWeightText = today.NetWeight.ToString("N0", CultureInfo.CurrentCulture);
        TodayTicketCount = today.TicketCount;
        TodayBreakdown = today.TicketCount == 0
            ? "Todavía no hay pesajes hoy"
            : $"{Plural(today.ConventionalCount, "convencional", "convencionales")} · " +
              $"{Plural(today.SingleCount, "único", "únicos")}";
    }

    // Mismo criterio que Configuracion: lo guardado contra lo que Windows detecta
    private async Task LoadDevicesAsync()
    {
        try
        {
            // WMI puede tardar: se consulta fuera del hilo de la interfaz
            var ports = await Task.Run(() => _portDiscovery.GetPorts());

            var detected = ports
                .Select(port => port.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            Indicator = HomeDeviceStatus.Create(
                "Indicador",
                _settings.GetValue("WeightIndicator:PortName"),
                detected);

            Printer = HomeDeviceStatus.Create(
                "Impresora",
                _settings.GetValue("Printer:PortName"),
                detected);
        }
        catch (Exception ex)
        {
            Indicator = new("Indicador · no se pudo revisar", StatusKind.Warning);
            Printer = new("Impresora · no se pudo revisar", StatusKind.Warning);

            _logger.LogError(ex, "No fue posible revisar los puertos en Inicio.");
        }
    }

    // Abre una ventana desde Inicio y, si se registro algo, avisa y recarga
    private async Task RunDialogAsync(Func<bool> showDialog)
    {
        SetDialogOpen(true);

        bool registered;

        try
        {
            registered = showDialog();
        }
        finally
        {
            SetDialogOpen(false);
        }

        if (registered)
            await RefreshAsync();
    }

    private void SetDialogOpen(bool isOpen)
    {
        _isDialogOpen = isOpen;

        NewWeighingCommand.NotifyCanExecuteChanged();
        RegisterTareCommand.NotifyCanExecuteChanged();
        RegisterAmountCommand.NotifyCanExecuteChanged();
    }

    private Task NewWeighingAsync()
    {
        return RunDialogAsync(() =>
        {
            var registration = _weighingDialogs.ShowCreateDialog();

            if (registration is null)
                return false;

            _notifications.ShowSuccess(
                $"Pesaje registrado en el folio {registration.TicketNumber}.");

            ShowPrintWarning(registration.TicketNumber, registration.PrintWarning);

            return true;
        });
    }

    private Task RegisterTareAsync(HomeInPlantRow row)
    {
        return RunDialogAsync(() =>
        {
            var registration = _weighingDialogs.ShowRegisterTareDialog(row.Ticket);

            if (registration is null)
                return false;

            _notifications.ShowSuccess(
                $"Tara registrada en el folio {registration.TicketNumber}: " +
                $"peso neto {registration.NetWeight:N0} kg.");

            ShowPrintWarning(registration.TicketNumber, registration.PrintWarning);

            return true;
        });
    }

    private Task RegisterAmountAsync(HomeAmountRow row)
    {
        return RunDialogAsync(() =>
        {
            var registration = _amountDialogs.ShowRegisterAmountDialog(row.Ticket);

            if (registration is null)
                return false;

            _notifications.ShowSuccess(
                $"Importe registrado en el folio {registration.TicketNumber}: " +
                $"${registration.Amount:N2}. El ticket quedó completado.");

            return true;
        });
    }

    // El registro si se hizo: el problema de impresion va en un aviso aparte
    private void ShowPrintWarning(string ticketNumber, string? printWarning)
    {
        if (printWarning is not null)
            _notifications.ShowWarning($"No se imprimió el folio {ticketNumber}. {printWarning}");
    }
}
