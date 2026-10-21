using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Devices;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.ViewModels.Settings;

// Configuracion: puertos de la impresora y del indicador, que se guardan al
// elegirlos, sus pruebas y el encabezado del ticket
public sealed class SettingsViewModel : ObservableObject
{
    // Campos

    private readonly ApplicationSettingsService _settingsService;
    private readonly ISerialPortDiscovery _portDiscovery;
    private readonly ITicketPrintService _printService;
    private readonly INotificationService _notifications;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SettingsViewModel> _logger;

    private bool _isRefreshing;
    private string _detectionSummary = string.Empty;
    private TicketHeaderSettings _ticketHeader = new();

    // Constructor

    public SettingsViewModel(
        ApplicationSettingsService settingsService,
        ISerialPortDiscovery portDiscovery,
        ITicketPrintService printService,
        INotificationService notifications,
        WeightCaptureViewModel indicatorReading,
        TimeProvider timeProvider,
        ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _portDiscovery = portDiscovery;
        _printService = printService;
        _notifications = notifications;
        _timeProvider = timeProvider;
        _logger = logger;

        IndicatorReading = indicatorReading;

        Printer = new DevicePortViewModel(
            "Impresora",
            "Epson TM-U295 · ticket de 35 caracteres por línea",
            "la impresora",
            "Printer",
            "Imprimir prueba",
            OnPortSelected,
            PrintTestAsync);

        WeightIndicator = new DevicePortViewModel(
            "Indicador de peso",
            "Dini Argeo · lectura de peso estable",
            "el indicador",
            "WeightIndicator",
            "Leer peso",
            OnPortSelected,
            ReadWeightAsync);

        RefreshPortsCommand = new AsyncRelayCommand(
            RefreshPortsAsync,
            () => !IsRefreshing);
    }

    // Propiedades

    public string Title => "Configuración";

    public string Description =>
        "Dispositivos conectados a la báscula y datos que aparecen en los tickets";

    // Puertos que Windows detecta en este momento
    public ObservableCollection<PortOption> AvailablePorts { get; } = new();

    // Sin puertos el selector se deshabilita: abrir una lista vacia confunde
    public bool HasAvailablePorts => AvailablePorts.Count > 0;

    public string PortPlaceholder =>
        IsRefreshing ? "Buscando puertos…"
        : HasAvailablePorts ? "Selecciona un puerto"
        : "No se detectó ningún puerto";

    public DevicePortViewModel Printer { get; }

    public DevicePortViewModel WeightIndicator { get; }

    // Misma lectura del indicador que en Pesaje, con sus mensajes de error
    public WeightCaptureViewModel IndicatorReading { get; }

    public string DetectionSummary
    {
        get => _detectionSummary;
        private set => SetProperty(ref _detectionSummary, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        private set
        {
            if (!SetProperty(ref _isRefreshing, value))
                return;

            OnPropertyChanged(nameof(PortPlaceholder));
            RefreshPortsCommand?.NotifyCanExecuteChanged();
        }
    }

    public TicketHeaderSettings TicketHeader
    {
        get => _ticketHeader;
        private set
        {
            if (SetProperty(ref _ticketHeader, value))
                OnPropertyChanged(nameof(HeaderPreview));
        }
    }

    public string HeaderPreview => TicketHeader.ToPreview();

    // Comandos

    public AsyncRelayCommand RefreshPortsCommand { get; }

    // Metodos publicos

    // Al entrar a la pantalla: lo guardado y los puertos conectados ahora
    public async Task LoadAsync()
    {
        try
        {
            Printer.Initialize(_settingsService.GetValue("Printer:PortName"));
            WeightIndicator.Initialize(_settingsService.GetValue("WeightIndicator:PortName"));

            TicketHeader = _settingsService.GetTicketHeader();
        }
        catch (Exception ex)
        {
            _notifications.ShowError("No fue posible cargar la configuración.");

            _logger.LogError(ex, "No fue posible cargar la configuración.");
        }

        await RefreshPortsAsync();
    }

    // Metodos privados

    private async Task RefreshPortsAsync()
    {
        IsRefreshing = true;

        try
        {
            // WMI puede tardar: se consulta fuera del hilo de la interfaz
            var ports = await Task.Run(() => _portDiscovery.GetPorts());

            AvailablePorts.Clear();

            foreach (var port in ports)
            {
                AvailablePorts.Add(new PortOption(
                    port.Name,
                    port.Description is null ? port.Name : $"{port.Name} · {port.Description}"));
            }

            OnPropertyChanged(nameof(HasAvailablePorts));
            OnPropertyChanged(nameof(PortPlaceholder));

            var detected = ports
                .Select(port => port.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Despues de llenar las opciones: el selector necesita que ya existan
            Printer.UpdateDetection(detected);
            WeightIndicator.UpdateDetection(detected);

            string time = _timeProvider.GetLocalNow().ToString("HH:mm", CultureInfo.CurrentCulture);

            DetectionSummary = ports.Count switch
            {
                0 => $"No se detectó ningún puerto · revisado a las {time}",
                1 => $"1 puerto detectado · revisado a las {time}",
                _ => $"{ports.Count} puertos detectados · revisado a las {time}"
            };
        }
        catch (Exception ex)
        {
            DetectionSummary = "No fue posible consultar los puertos.";

            _logger.LogError(ex, "No fue posible consultar los puertos serie.");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    // Se guarda en cuanto el operador elige: no hay boton Guardar que olvidar
    private void OnPortSelected(DevicePortViewModel device)
    {
        var other = ReferenceEquals(device, Printer) ? WeightIndicator : Printer;
        string port = device.SelectedPort!;

        // Dos dispositivos no pueden compartir un puerto: ninguno funcionaria
        if (string.Equals(port, other.SavedPort, StringComparison.OrdinalIgnoreCase))
        {
            device.RejectSelection($"{port} ya lo usa {other.DeviceName}.");
            return;
        }

        try
        {
            _settingsService.SavePort(device.SectionName, port);
        }
        catch (Exception ex)
        {
            _notifications.ShowError($"No fue posible guardar el puerto de {device.DeviceName}.");

            _logger.LogError(
                ex,
                "No fue posible guardar el puerto {Port} en {Section}.",
                port,
                device.SectionName);

            device.RevertSelection();
            return;
        }

        device.MarkSaved(port);

        _notifications.ShowSuccess($"Puerto guardado para {device.DeviceName}: {port}.");
    }

    private async Task<DeviceTestResult> PrintTestAsync()
    {
        string? printWarning = await _printService.PrintTestAsync();

        string time = _timeProvider.GetLocalNow().ToString("HH:mm", CultureInfo.CurrentCulture);

        return printWarning is null
            ? new DeviceTestResult(true, $"Prueba enviada a las {time}. Revisa el ticket impreso.")
            : new DeviceTestResult(false, printWarning);
    }

    private async Task<DeviceTestResult> ReadWeightAsync()
    {
        await IndicatorReading.ReadAsync();

        return IndicatorReading.HasReading
            ? new DeviceTestResult(true, $"Lectura correcta: {IndicatorReading.WeightDisplay} kg.")
            : new DeviceTestResult(false, IndicatorReading.ReadErrorMessage);
    }
}