using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.Settings;

// Puerto de un dispositivo: el guardado, el seleccionado, si Windows lo
// detecta y su prueba. La impresora y el indicador usan la misma tarjeta
public sealed class DevicePortViewModel : ValidatableObject
{
    // Campos

    private readonly Action<DevicePortViewModel> _onPortSelected;
    private readonly Func<Task<DeviceTestResult>> _runTest;

    private string? _savedPort;
    private string? _selectedPort;
    private bool _isConnected;
    private bool _isTesting;
    private DeviceTestResult? _testResult;

    // Evita guardar cuando el cambio de seleccion viene del codigo y no del operador
    private bool _isUpdatingSelection;

    // Constructor

    public DevicePortViewModel(
        string title,
        string description,
        string deviceName,
        string sectionName,
        string testButtonText,
        Action<DevicePortViewModel> onPortSelected,
        Func<Task<DeviceTestResult>> runTest)
    {
        Title = title;
        Description = description;
        DeviceName = deviceName;
        SectionName = sectionName;
        TestButtonText = testButtonText;

        _onPortSelected = onPortSelected;
        _runTest = runTest;

        TestCommand = new AsyncRelayCommand(RunTestAsync, () => CanTest);
    }

    // Propiedades

    public string Title { get; }

    public string Description { get; }

    // Para los mensajes: "la impresora" o "el indicador"
    public string DeviceName { get; }

    // Seccion de la configuracion: "Printer" o "WeightIndicator"
    public string SectionName { get; }

    public bool IsPrinter => SectionName == "Printer";

    public string TestButtonText { get; }

    public string? SavedPort
    {
        get => _savedPort;
        private set => SetProperty(ref _savedPort, value);
    }

    // Lo que muestra el selector. El operador lo cambia; el codigo lo
    // sincroniza con lo guardado sin disparar otro guardado
    public string? SelectedPort
    {
        get => _selectedPort;
        set
        {
            if (!SetProperty(ref _selectedPort, value))
                return;

            // Al reemplazar las opciones el selector manda null: no es una eleccion
            if (_isUpdatingSelection || value is null)
                return;

            // Volver al puerto ya guardado no cambia nada, pero quita un error previo
            if (value == SavedPort)
            {
                ClearErrors();
                return;
            }

            _onPortSelected(this);
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (SetProperty(ref _isConnected, value))
                NotifyStatusChanged();
        }
    }

    public bool IsTesting
    {
        get => _isTesting;
        private set
        {
            if (SetProperty(ref _isTesting, value))
                NotifyStatusChanged();
        }
    }

    // Sin puerto el dispositivo no funciona en Pesaje, por eso es error;
    // un puerto guardado pero desconectado suele ser un cable, por eso es advertencia
    public StatusKind StatusKind =>
        SavedPort is null ? StatusKind.Error
        : IsConnected ? StatusKind.Success
        : StatusKind.Warning;

    public string StatusText =>
        SavedPort is null ? "No configurado"
        : IsConnected ? "Conectado"
        : $"{SavedPort} no está conectado";

    // Linea de ayuda de la tarjeta: el resultado de la prueba, o que hacer
    public string HelpText =>
        IsTesting ? "Probando…"
        : _testResult is not null ? _testResult.Message
        : SavedPort is null ? $"Conecta {DeviceName} y elige su puerto."
        : !IsConnected ? $"Se guardó {SavedPort}, pero Windows no lo detecta. Revisa el cable y presiona Actualizar puertos, o elige otro puerto."
        : "Usa la prueba para confirmar que el dispositivo responde.";

    // El color de la ayuda acompana al de la etiqueta mientras no hay prueba
    public StatusKind HelpKind =>
        IsTesting ? StatusKind.Neutral
        : _testResult is not null ? (_testResult.Succeeded ? StatusKind.Success : StatusKind.Error)
        : SavedPort is null ? StatusKind.Error
        : !IsConnected ? StatusKind.Warning
        : StatusKind.Neutral;

    // Solo se prueba un puerto guardado y conectado
    public bool CanTest => IsConnected && !IsTesting;

    // Comandos

    public AsyncRelayCommand TestCommand { get; }

    // Metodos publicos

    public void Initialize(string? savedPort)
    {
        SavedPort = string.IsNullOrWhiteSpace(savedPort)
            ? null
            : savedPort.Trim().ToUpperInvariant();

        NotifyStatusChanged();
    }

    // Despues de consultar Windows: un puerto guardado que no se detecta no se
    // muestra como seleccionado, para no ofrecer algo que no existe
    public void UpdateDetection(IReadOnlySet<string> detectedPorts)
    {
        IsConnected = SavedPort is not null && detectedPorts.Contains(SavedPort);

        SetSelectionSilently(IsConnected ? SavedPort : null);
        ClearErrors();

        _testResult = null;
        NotifyStatusChanged();
    }

    public void MarkSaved(string port)
    {
        SavedPort = port;
        IsConnected = true;

        _testResult = null;
        ClearErrors();
        NotifyStatusChanged();
    }

    public void RejectSelection(string message)
    {
        SetError(nameof(SelectedPort), message);
    }

    // Regresa el selector a lo guardado, por ejemplo si no se pudo guardar
    public void RevertSelection()
    {
        SetSelectionSilently(IsConnected ? SavedPort : null);
    }

    // Metodos privados

    private void SetSelectionSilently(string? port)
    {
        _isUpdatingSelection = true;

        try
        {
            SelectedPort = port;
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    private async Task RunTestAsync()
    {
        IsTesting = true;

        try
        {
            _testResult = await _runTest();
        }
        finally
        {
            IsTesting = false;
        }

        NotifyStatusChanged();
    }

    private void NotifyStatusChanged()
    {
        OnPropertyChanged(nameof(StatusKind));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(HelpText));
        OnPropertyChanged(nameof(HelpKind));
        OnPropertyChanged(nameof(CanTest));
        TestCommand?.NotifyCanExecuteChanged();
    }
}