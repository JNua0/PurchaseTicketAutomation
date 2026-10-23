using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterDepartureWeight;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Forms;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Exceptions;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Registrar tara (CU-02): pesaje de salida de un convencional en planta.
// Muestra los datos de entrada y calcula el neto mientras se captura la tara
public class RegisterTareViewModel : ObservableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITicketPrintService _printService;
    private readonly ILogger<RegisterTareViewModel> _logger;
    private WeighingListItem? _ticket;
    private IReadOnlyList<DetailItem> _details = [];
    private bool _printOnSave = true;
    private string _errorMessage = string.Empty;
    private bool _isSaving;

    // Constructor

    public RegisterTareViewModel(
        IServiceScopeFactory scopeFactory,
        WeightCaptureViewModel tare,
        ITicketPrintService printService,
        ILogger<RegisterTareViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _printService = printService;
        _logger = logger;

        Tare = tare;

        // El neto se recalcula con cada cambio de la tara
        Tare.PropertyChanged += OnTarePropertyChanged;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    // Eventos

    // Avisa a la ventana que la tara se registro para que se cierre
    public event EventHandler? Registered;

    // Propiedades

    public WeightCaptureViewModel Tare { get; }

    public string Description =>
        $"Pesaje de salida del ticket folio {_ticket?.TicketNumber}";

    // Datos de entrada de solo lectura
    public IReadOnlyList<DetailItem> Details
    {
        get => _details;
        private set => SetProperty(ref _details, value);
    }

    // Peso bruto − tara = peso neto
    public IReadOnlyList<BreakdownStep> Breakdown
    {
        get
        {
            decimal grossWeight = GrossWeight;
            decimal? tareWeight = Tare.Weight;

            // Sin tara, o con una tara que no es menor al bruto, aun no hay neto.
            // Es solo para mostrar: la regla real la valida el dominio al registrar
            string netWeight =
                tareWeight is decimal tare && tare < grossWeight
                    ? DisplayFormat.WholeKg(grossWeight - tare)
                    : "—";

            return
            [
                new("Peso bruto", DisplayFormat.WholeKg(grossWeight)),
                new("− Tara", tareWeight is decimal value ? DisplayFormat.WholeKg(value) : "—"),
                new("= Peso neto", netWeight, IsResult: true)
            ];
        }
    }

    public bool PrintOnSave
    {
        get => _printOnSave;
        set => SetProperty(ref _printOnSave, value);
    }

    // Errores que no pertenecen a un campo, como un ticket que ya no esta en planta
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (SetProperty(ref _isSaving, value))
                NotifyBusyChanged();
        }
    }

    public bool IsBusy => IsSaving || Tare.IsReading;

    // Resultado del registro; el servicio de dialogo lo lee al cerrar
    public TareRegistration? Registration { get; private set; }

    // Comandos

    public AsyncRelayCommand SaveCommand { get; }

    // Propiedades privadas

    // Un convencional en planta siempre tiene peso bruto
    private decimal GrossWeight => _ticket?.GrossWeight ?? 0;

    // Metodos publicos

    // Los datos vienen de la fila de la lista: no hace falta otra consulta
    public void Initialize(WeighingListItem ticket)
    {
        _ticket = ticket;

        Details =
        [
            new("Proveedor", ticket.SupplierName),
            new("Material", ticket.MaterialName),
            new("Placas", ticket.LicensePlate ?? "Sin placas"),
            new("Chofer", ticket.Transporter),
            new("Entrada", ticket.CheckInAt.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture)),
            new("Peso bruto", DisplayFormat.WholeKg(GrossWeight))
        ];

        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(Breakdown));
    }

    // Metodos privados

    private bool CanSave()
    {
        return _ticket is not null && !IsBusy;
    }

    private async Task SaveAsync()
    {
        if (!CanSave())
            return;

        ErrorMessage = string.Empty;

        if (!Tare.TryGetWeight(out decimal tareWeight))
            return;

        IsSaving = true;

        try
        {
            if (!await TryRegisterAsync(tareWeight))
                return;

            // La tara ya esta guardada: si la impresion falla se avisa,
            // pero el registro no se deshace
            string? printWarning = PrintOnSave
                ? await _printService.PrintAsync(_ticket!.Id)
                : null;

            Registration = new TareRegistration(
                _ticket!.TicketNumber,
                GrossWeight - tareWeight,
                printWarning);
        }
        finally
        {
            IsSaving = false;
        }

        // Fuera del try: un error al cerrar la ventana no debe
        // mostrarse como error al registrar
        Registered?.Invoke(this, EventArgs.Empty);
    }

    // Devuelve false si no se registro; el motivo ya queda en pantalla
    private async Task<bool> TryRegisterAsync(decimal tareWeight)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            await scope.ServiceProvider
                .GetRequiredService<RegisterDepartureWeight>()
                .ExecuteAsync(new RegisterDepartureWeightCommand(
                    _ticket!.Id,
                    tareWeight));

            return true;
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            // Por ejemplo, otro equipo ya registro la tara o cancelo el ticket
            ErrorMessage = ex.Message;
        }
        catch (ArgumentException ex)
        {
            // Las reglas de la tara: mayor a cero y menor al peso bruto
            Tare.SetError(ExceptionMessages.ForUser(ex));
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible registrar la tara.";

            _logger.LogError(
                ex,
                "No fue posible registrar la tara del ticket {TicketId}.",
                _ticket!.Id);
        }

        return false;
    }

    private void OnTarePropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WeightCaptureViewModel.Weight))
            OnPropertyChanged(nameof(Breakdown));

        if (e.PropertyName == nameof(WeightCaptureViewModel.IsReading))
            NotifyBusyChanged();
    }

    private void NotifyBusyChanged()
    {
        OnPropertyChanged(nameof(IsBusy));
        SaveCommand?.NotifyCanExecuteChanged();
    }
}