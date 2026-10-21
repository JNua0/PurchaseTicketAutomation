using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.UseCases.Weighing.ReadWeight;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Captura de un peso: lectura del indicador o captura manual.
// La comparten Nuevo pesaje, Registrar tara y la correccion de tickets,
// asi la lectura y sus errores se resuelven en un solo lugar
public sealed class WeightCaptureViewModel : ObservableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeightCaptureViewModel> _logger;

    private WeightCaptureState _state = WeightCaptureState.Empty;
    private decimal? _readWeight;
    private string _manualText = string.Empty;
    private string _readErrorMessage = string.Empty;
    private string _validationError = string.Empty;

    // Constructor

    public WeightCaptureViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger<WeightCaptureViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        ReadCommand = new AsyncRelayCommand(ReadAsync);
        CaptureManuallyCommand = new RelayCommand(CaptureManually);
    }

    // Propiedades

    public WeightCaptureState State
    {
        get => _state;
        private set
        {
            if (!SetProperty(ref _state, value))
                return;

            // Todo lo que la vista muestra depende del estado
            OnPropertyChanged(nameof(IsReading));
            OnPropertyChanged(nameof(IsManual));
            OnPropertyChanged(nameof(HasReading));
            OnPropertyChanged(nameof(HasReadError));
            OnPropertyChanged(nameof(ShowsWeightText));
            OnPropertyChanged(nameof(ShowsCaptureManually));
            OnPropertyChanged(nameof(ReadButtonText));
            OnPropertyChanged(nameof(Weight));
            OnPropertyChanged(nameof(WeightDisplay));
        }
    }

    // Peso listo para usar, o null si aun no hay uno valido
    public decimal? Weight =>
        State switch
        {
            WeightCaptureState.Read => _readWeight,
            WeightCaptureState.Manual => ParseManual(_manualText),
            _ => null
        };

    public string WeightDisplay =>
        Weight is decimal weight
            ? weight.ToString("N0", CultureInfo.CurrentCulture)
            : "—";

    // Texto que escribe el operador en captura manual
    public string ManualText
    {
        get => _manualText;
        set
        {
            if (!SetProperty(ref _manualText, value))
                return;

            // Al corregir el peso deja de tener sentido el error anterior
            ValidationError = string.Empty;

            OnPropertyChanged(nameof(Weight));
            OnPropertyChanged(nameof(WeightDisplay));
        }
    }

    // Motivo por el que fallo la lectura del indicador
    public string ReadErrorMessage
    {
        get => _readErrorMessage;
        private set => SetProperty(ref _readErrorMessage, value);
    }

    // Error del campo, como falta de peso o una tara mayor al bruto
    public string ValidationError
    {
        get => _validationError;
        private set
        {
            if (SetProperty(ref _validationError, value))
                OnPropertyChanged(nameof(HasValidationError));
        }
    }

    public bool HasValidationError => !string.IsNullOrEmpty(ValidationError);

    public bool IsReading => State == WeightCaptureState.Reading;

    public bool IsManual => State == WeightCaptureState.Manual;

    public bool HasReading => State == WeightCaptureState.Read;

    public bool HasReadError => State == WeightCaptureState.Error;

    // En captura manual el peso se escribe en un campo en lugar de mostrarse
    public bool ShowsWeightText => !IsManual;

    public bool ShowsCaptureManually =>
        State is WeightCaptureState.Empty
            or WeightCaptureState.Read
            or WeightCaptureState.Error;

    public string ReadButtonText =>
        State switch
        {
            WeightCaptureState.Reading => "Leyendo…",
            WeightCaptureState.Read => "Leer de nuevo",
            WeightCaptureState.Error => "Reintentar",
            WeightCaptureState.Manual => "Leer del indicador",
            _ => "Leer indicador"
        };

    // Comandos

    public AsyncRelayCommand ReadCommand { get; }

    public RelayCommand CaptureManuallyCommand { get; }

    // Metodos publicos

    // Valida que exista un peso. El rango (mayor a cero, tara menor al
    // bruto) lo valida el dominio y el formulario lo muestra con SetError
    public bool TryGetWeight(out decimal weight)
    {
        if (Weight is decimal value)
        {
            ValidationError = string.Empty;
            weight = value;
            return true;
        }

        ValidationError =
            IsManual && !string.IsNullOrWhiteSpace(ManualText)
                ? "Escribe el peso en kilogramos enteros, sin decimales."
                : "Lee el peso del indicador o captúralo manualmente.";

        weight = 0;
        return false;
    }

    // Muestra en el campo un error detectado fuera de el, como el del dominio
    public void SetError(string message)
    {
        ValidationError = message;
    }

    // Al editar un ticket, el peso guardado se muestra como captura manual
    public void SetManualWeight(decimal weight)
    {
        ManualText = weight.ToString("0", CultureInfo.CurrentCulture);
        State = WeightCaptureState.Manual;
    }

    public void Reset()
    {
        _readWeight = null;
        ManualText = string.Empty;
        ReadErrorMessage = string.Empty;
        ValidationError = string.Empty;
        State = WeightCaptureState.Empty;
    }

    // Metodos privados

    // Solo enteros con separador de miles opcional: "13200" o "13,200"
    private static decimal? ParseManual(string text)
    {
        bool parsed = decimal.TryParse(
            text.Trim(),
            NumberStyles.AllowThousands,
            CultureInfo.CurrentCulture,
            out decimal weight);

        return parsed ? weight : null;
    }

    private void CaptureManually()
    {
        // Se parte del peso leido, si lo hay, para solo ajustarlo
        if (_readWeight is decimal readWeight && State == WeightCaptureState.Read)
            ManualText = readWeight.ToString("0", CultureInfo.CurrentCulture);

        ValidationError = string.Empty;
        State = WeightCaptureState.Manual;
    }

    private async Task ReadAsync()
    {
        ReadErrorMessage = string.Empty;
        ValidationError = string.Empty;
        State = WeightCaptureState.Reading;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            decimal weight = await scope.ServiceProvider
                .GetRequiredService<ReadWeightUseCase>()
                .ExecuteAsync();

            if (weight <= 0)
            {
                Fail("El indicador devolvió un peso inválido.", null);
                return;
            }

            _readWeight = weight;
            State = WeightCaptureState.Read;
        }
        catch (TimeoutException ex)
        {
            Fail("El indicador no respondió a tiempo.", ex);
        }
        catch (IOException ex)
        {
            Fail("Ocurrió un error de comunicación con el indicador.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            Fail("No fue posible acceder al puerto del indicador.", ex);
        }
        catch (Exception ex)
        {
            Fail("No fue posible obtener el peso del indicador.", ex);
        }
    }

    // Un indicador apagado no es una falla de la aplicacion, por eso es
    // advertencia; aun asi el registro ayuda a diagnosticar el puerto
    private void Fail(string message, Exception? exception)
    {
        _readWeight = null;
        ReadErrorMessage = message;
        State = WeightCaptureState.Error;

        _logger.LogWarning(
            exception,
            "Falló la lectura del indicador: {Reason}",
            message);
    }
}