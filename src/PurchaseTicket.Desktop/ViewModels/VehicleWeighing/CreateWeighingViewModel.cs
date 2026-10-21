using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Materials.GetActive;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateConventional;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateSingle;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Results;
using PurchaseTicket.Application.UseCases.Suppliers.GetActive;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.Exceptions;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Formulario de Nuevo pesaje: registra la entrada de un convencional o un
// pesaje unico. La lectura del peso la resuelve WeightCaptureViewModel
public class CreateWeighingViewModel : ValidatableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITicketPrintService _printService;
    private readonly ILogger<CreateWeighingViewModel> _logger;

    private WeighingType _selectedWeighingType = WeighingType.Conventional;
    private int? _selectedSupplierId;
    private int? _selectedMaterialId;
    private string _licensePlate = string.Empty;
    private string _transporter = string.Empty;
    private bool _printOnSave = true;
    private string _errorMessage = string.Empty;
    private bool _isLoadingCatalogs;
    private bool _isSaving;

    // Se activa tras el primer intento de registrar; desde ahi se valida al escribir
    private bool _validateOnChange;

    // Constructor

    public CreateWeighingViewModel(
        IServiceScopeFactory scopeFactory,
        WeightCaptureViewModel weight,
        ITicketPrintService printService,
        ILogger<CreateWeighingViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _printService = printService;
        _logger = logger;

        Weight = weight;

        // Mientras se lee el indicador no se permite registrar ni cerrar
        Weight.PropertyChanged += OnWeightPropertyChanged;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    // Eventos

    // Avisa a la ventana que el ticket se registro para que se cierre
    public event EventHandler? Registered;

    // Propiedades

    public IReadOnlyList<WeighingTypeOption> WeighingTypeOptions { get; } =
    [
        new(WeighingType.Conventional, "Convencional", "Entrada y salida · bruto − tara"),
        new(WeighingType.Single, "Pesaje único", "Una sola medición · peso neto")
    ];

    public ObservableCollection<SupplierOptionDto> Suppliers { get; } = new();

    public ObservableCollection<MaterialOptionDto> Materials { get; } = new();

    // Peso bruto en el convencional, peso neto en el unico
    public WeightCaptureViewModel Weight { get; }

    public WeighingType SelectedWeighingType
    {
        get => _selectedWeighingType;
        set
        {
            if (!SetProperty(ref _selectedWeighingType, value))
                return;

            OnPropertyChanged(nameof(IsConventional));
            OnPropertyChanged(nameof(WeightLabel));
            OnPropertyChanged(nameof(WeightSectionTitle));

            // Un peso bruto no sirve como neto ni al reves: se vuelve a capturar
            Weight.Reset();

            // Las placas pasan de obligatorias a opcionales, o al reves
            if (_validateOnChange)
                ValidateLicensePlate();
        }
    }

    public bool IsConventional =>
        SelectedWeighingType == WeighingType.Conventional;

    public string WeightSectionTitle =>
        IsConventional ? "PESO DE ENTRADA" : "PESO CAPTURADO";

    public string WeightLabel =>
        IsConventional ? "Peso bruto" : "Peso neto";

    public int? SelectedSupplierId
    {
        get => _selectedSupplierId;
        set
        {
            if (SetProperty(ref _selectedSupplierId, value) && _validateOnChange)
                ValidateSupplier();
        }
    }

    public int? SelectedMaterialId
    {
        get => _selectedMaterialId;
        set
        {
            if (SetProperty(ref _selectedMaterialId, value) && _validateOnChange)
                ValidateMaterial();
        }
    }

    public string LicensePlate
    {
        get => _licensePlate;
        set
        {
            if (SetProperty(ref _licensePlate, value) && _validateOnChange)
                ValidateLicensePlate();
        }
    }

    public string Transporter
    {
        get => _transporter;
        set
        {
            if (SetProperty(ref _transporter, value) && _validateOnChange)
                ValidateTransporter();
        }
    }

    public bool PrintOnSave
    {
        get => _printOnSave;
        set => SetProperty(ref _printOnSave, value);
    }

    // Errores que no pertenecen a un campo, como fallas de conexion
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoadingCatalogs
    {
        get => _isLoadingCatalogs;
        private set
        {
            if (SetProperty(ref _isLoadingCatalogs, value))
                NotifyBusyChanged();
        }
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

    public bool IsBusy =>
        IsLoadingCatalogs || IsSaving || Weight.IsReading;

    // Resultado del registro; el servicio de dialogo lo lee al cerrar
    public WeighingRegistration? Registration { get; private set; }

    // Comandos

    public AsyncRelayCommand SaveCommand { get; }

    // Metodos publicos

    public async Task LoadCatalogsAsync()
    {
        IsLoadingCatalogs = true;
        ErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var suppliers = await scope.ServiceProvider
                .GetRequiredService<GetActiveSuppliers>()
                .ExecuteAsync();

            var materials = await scope.ServiceProvider
                .GetRequiredService<GetActiveMaterials>()
                .ExecuteAsync();

            Suppliers.Clear();
            Materials.Clear();

            foreach (var supplier in suppliers)
                Suppliers.Add(supplier);

            foreach (var material in materials)
                Materials.Add(material);
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "No fue posible cargar los proveedores y materiales.";

            _logger.LogError(
                ex,
                "No fue posible cargar los catálogos del nuevo pesaje.");
        }
        finally
        {
            IsLoadingCatalogs = false;
        }
    }

    // Metodos privados

    private bool CanSave()
    {
        return !IsBusy;
    }

    private async Task SaveAsync()
    {
        if (!CanSave())
            return;

        ErrorMessage = string.Empty;
        _validateOnChange = true;

        // Las reglas se revisan antes de ir a la base de datos
        if (!ValidateAll(out decimal weight))
            return;

        IsSaving = true;

        try
        {
            CreatePurchaseTicketResult? result = await TryCreateAsync(weight);

            if (result is null)
                return;

            // El ticket ya esta guardado: si la impresion falla se avisa,
            // pero el registro no se deshace; se puede reimprimir desde la lista
            string? printWarning = PrintOnSave
                ? await _printService.PrintAsync(result.Id)
                : null;

            Registration = new WeighingRegistration(
                result.TicketNumber,
                SelectedWeighingType,
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

    // Devuelve null si no se registro; el motivo ya queda en pantalla
    private async Task<CreatePurchaseTicketResult?> TryCreateAsync(decimal weight)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            return IsConventional
                ? await CreateConventionalAsync(scope.ServiceProvider, weight)
                : await CreateSingleAsync(scope.ServiceProvider, weight);
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            // Por ejemplo, un proveedor que se desactivo con el formulario abierto
            ErrorMessage = ex.Message;
        }
        catch (ArgumentException ex)
        {
            // Placas y transportista ya se validaron; lo que queda es el rango del peso
            Weight.SetError(GetUserMessage(ex));
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible registrar el pesaje.";

            _logger.LogError(
                ex,
                "No fue posible registrar un pesaje {WeighingType}.",
                SelectedWeighingType);
        }

        return null;
    }

    private Task<CreatePurchaseTicketResult> CreateConventionalAsync(
        IServiceProvider services,
        decimal grossWeight)
    {
        return services
            .GetRequiredService<CreateConventionalPurchaseTicket>()
            .ExecuteAsync(
                new CreateConventionalPurchaseTicketCommand(
                    SelectedSupplierId!.Value,
                    SelectedMaterialId!.Value,
                    LicensePlate,
                    Transporter,
                    grossWeight));
    }

    private Task<CreatePurchaseTicketResult> CreateSingleAsync(
        IServiceProvider services,
        decimal netWeight)
    {
        // En el pesaje unico las placas son opcionales
        string? licensePlate = string.IsNullOrWhiteSpace(LicensePlate)
            ? null
            : LicensePlate;

        return services
            .GetRequiredService<CreateSinglePurchaseTicket>()
            .ExecuteAsync(
                new CreateSinglePurchaseTicketCommand(
                    SelectedSupplierId!.Value,
                    SelectedMaterialId!.Value,
                    licensePlate,
                    Transporter,
                    netWeight));
    }

    // Se validan todos los campos para mostrar todos los errores a la vez
    private bool ValidateAll(out decimal weight)
    {
        bool isSupplierValid = ValidateSupplier();
        bool isMaterialValid = ValidateMaterial();
        bool isLicensePlateValid = ValidateLicensePlate();
        bool isTransporterValid = ValidateTransporter();
        bool hasWeight = Weight.TryGetWeight(out weight);

        return isSupplierValid
            && isMaterialValid
            && isLicensePlateValid
            && isTransporterValid
            && hasWeight;
    }

    private bool ValidateSupplier()
    {
        string? error = SelectedSupplierId is null
            ? "Selecciona un proveedor."
            : null;

        SetError(nameof(SelectedSupplierId), error);

        return error is null;
    }

    private bool ValidateMaterial()
    {
        string? error = SelectedMaterialId is null
            ? "Selecciona un material."
            : null;

        SetError(nameof(SelectedMaterialId), error);

        return error is null;
    }

    // Mismas reglas del dominio: obligatorias en el convencional, opcionales en el unico
    private bool ValidateLicensePlate()
    {
        string? error = IsConventional
            ? ValidateRule(() => Ticket.NormalizeLicensePlate(LicensePlate))
            : ValidateRule(() => Ticket.NormalizeOptionalLicensePlate(LicensePlate));

        SetError(nameof(LicensePlate), error);

        return error is null;
    }

    private bool ValidateTransporter()
    {
        string? error = ValidateRule(
            () => Ticket.NormalizeTransporter(Transporter));

        SetError(nameof(Transporter), error);

        return error is null;
    }

    private void OnWeightPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WeightCaptureViewModel.IsReading))
            NotifyBusyChanged();
    }

    private void NotifyBusyChanged()
    {
        OnPropertyChanged(nameof(IsBusy));
        SaveCommand?.NotifyCanExecuteChanged();
    }
}