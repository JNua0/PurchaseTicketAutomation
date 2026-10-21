using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Configuration;

namespace PurchaseTicket.Desktop.ViewModels.Settings;

public class SettingsViewModel : INotifyPropertyChanged
{
    // Dependencias
    private readonly ApplicationSettingsService _settingsService;
    private readonly ILogger<SettingsViewModel> _logger;

    // Puertos detectados por Windows
    private readonly HashSet<string> _detectedPorts =
        new(StringComparer.OrdinalIgnoreCase);

    // Estado de los dispositivos
    private string? _selectedPrinterPort;
    private string? _selectedWeightIndicatorPort;

    // Mensajes de la interfaz
    private string _successMessage = string.Empty;
    private string _errorMessage = string.Empty;

    public SettingsViewModel(
        ApplicationSettingsService settingsService,
        ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _logger = logger;

        RefreshPortsCommand = new RelayCommand(RefreshPorts);
        SaveSettingsCommand = new RelayCommand(SaveSettings);

        LoadSettings();
    }

    // Informacion general
    public string Title => "Configuración";

    public string Description =>
        "Administra los dispositivos y parámetros del sistema.";

    // Colecciones
    public ObservableCollection<string> AvailablePorts { get; } = new();

    // Datos del encabezado del ticket
    public TicketHeaderSettings TicketHeader { get; } = new();

    // Comandos
    public ICommand RefreshPortsCommand { get; }

    public ICommand SaveSettingsCommand { get; }

    // Configuracion de la impresora
    public string? SelectedPrinterPort
    {
        get => _selectedPrinterPort;
        set
        {
            if (_selectedPrinterPort == value)
                return;

            _selectedPrinterPort = value;

            OnPropertyChanged(nameof(SelectedPrinterPort));
            NotifyPortStatusChanged();
        }
    }

    // Configuracion del indicador
    public string? SelectedWeightIndicatorPort
    {
        get => _selectedWeightIndicatorPort;
        set
        {
            if (_selectedWeightIndicatorPort == value)
                return;

            _selectedWeightIndicatorPort = value;

            OnPropertyChanged(nameof(SelectedWeightIndicatorPort));
            NotifyPortStatusChanged();
        }
    }

    // Disponibilidad de puertos
    public bool HasAvailablePorts => _detectedPorts.Count > 0;

    public bool IsPrinterPortAvailable =>
        !string.IsNullOrWhiteSpace(SelectedPrinterPort) &&
        _detectedPorts.Contains(SelectedPrinterPort);

    public bool IsWeightIndicatorPortAvailable =>
        !string.IsNullOrWhiteSpace(SelectedWeightIndicatorPort) &&
        _detectedPorts.Contains(SelectedWeightIndicatorPort);

    // Mensajes de disponibilidad
    public string PrinterPortStatus =>
        GetPortStatus(
            SelectedPrinterPort,
            "la impresora");

    public string WeightIndicatorPortStatus =>
        GetPortStatus(
            SelectedWeightIndicatorPort,
            "el indicador");

    // Mensaje de confirmacion
    public string SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (_successMessage == value)
                return;

            _successMessage = value;

            OnPropertyChanged(nameof(SuccessMessage));
        }
    }

    // Mensaje de error
    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value)
                return;

            _errorMessage = value;

            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Cargar la configuracion inicial
    private void LoadSettings()
    {
        try
        {
            LoadDeviceSettings();
            LoadTicketHeader();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "No fue posible cargar la configuración.";

            _logger.LogError(ex, "No fue posible cargar la configuración.");
        }

        // Consultar los puertos independientemente de la carga
        RefreshPorts();
    }

    // Cargar los puertos configurados
    private void LoadDeviceSettings()
    {
        SelectedPrinterPort =
            _settingsService.GetValue("Printer:PortName");

        SelectedWeightIndicatorPort =
            _settingsService.GetValue("WeightIndicator:PortName");
    }

    // Cargar la informacion del encabezado
    private void LoadTicketHeader()
    {
        TicketHeader.CompanyName =
            _settingsService.GetValue("TicketHeader:CompanyName")
            ?? string.Empty;

        TicketHeader.Address =
            _settingsService.GetValue("TicketHeader:Address")
            ?? string.Empty;

        TicketHeader.Neighborhood =
            _settingsService.GetValue("TicketHeader:Neighborhood")
            ?? string.Empty;

        TicketHeader.PhoneNumber =
            _settingsService.GetValue("TicketHeader:PhoneNumber")
            ?? string.Empty;

        TicketHeader.Email =
            _settingsService.GetValue("TicketHeader:Email")
            ?? string.Empty;
    }

    // Consultar los puertos reconocidos por Windows
    private void RefreshPorts()
    {
        SuccessMessage = string.Empty;

        try
        {
            var ports = SerialPort.GetPortNames()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    port => port,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

            UpdateAvailablePorts(ports);

            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "No fue posible consultar los puertos COM.";

            _logger.LogError(ex, "No fue posible consultar los puertos COM.");
        }
    }

    // Actualizar los puertos sin perder las selecciones
    private void UpdateAvailablePorts(IEnumerable<string> ports)
    {
        string? printerPort = SelectedPrinterPort;
        string? indicatorPort = SelectedWeightIndicatorPort;

        _detectedPorts.Clear();

        foreach (var port in ports)
        {
            _detectedPorts.Add(port);
        }

        // Agregar tambien los puertos configurados
        var allPorts = _detectedPorts
            .Concat(new[] { printerPort, indicatorPort }
                .OfType<string>())
            .Where(port => !string.IsNullOrWhiteSpace(port))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                port => port,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        AvailablePorts.Clear();

        foreach (var port in allPorts)
        {
            AvailablePorts.Add(port);
        }

        // Restaurar las selecciones
        SelectedPrinterPort = printerPort;
        SelectedWeightIndicatorPort = indicatorPort;

        OnPropertyChanged(nameof(HasAvailablePorts));
        NotifyPortStatusChanged();
    }

    // Guardar la configuracion general
    private void SaveSettings()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            _settingsService.SaveSettings(
                SelectedPrinterPort,
                SelectedWeightIndicatorPort,
                TicketHeader);

            SuccessMessage =
                "La configuración se guardó correctamente.";
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "No fue posible guardar la configuración.";

            _logger.LogError(ex, "No fue posible guardar la configuración.");
        }
    }

    // Obtener el mensaje de disponibilidad
    private string GetPortStatus(
        string? portName,
        string deviceName)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            return $"No hay un puerto configurado para {deviceName}.";
        }

        return _detectedPorts.Contains(portName)
            ? $"Windows detecta el puerto {portName}."
            : $"El puerto {portName} está configurado, pero Windows no lo detecta.";
    }

    // Notificar el cambio de disponibilidad
    private void NotifyPortStatusChanged()
    {
        OnPropertyChanged(nameof(IsPrinterPortAvailable));
        OnPropertyChanged(nameof(IsWeightIndicatorPortAvailable));
        OnPropertyChanged(nameof(PrinterPortStatus));
        OnPropertyChanged(nameof(WeightIndicatorPortStatus));
    }

    // Notificar cambios en las propiedades
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
