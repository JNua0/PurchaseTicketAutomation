using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Materials.GetActive;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;
using PurchaseTicket.Application.UseCases.Suppliers.GetActive;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Desktop.Components.Forms;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.Exceptions;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Corregir ticket (CU-06) desde la pantalla Tickets. Solo se habilitan los
// campos que permite el estado (matriz unificada) y los pesos siempre se
// capturan a mano (RN-07). Se envia solo lo que cambio
public sealed class CorrectTicketViewModel : ValidatableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CorrectTicketViewModel> _logger;

    private PurchaseTicketDetail? _ticket;
    private int? _selectedSupplierId;
    private int? _selectedMaterialId;
    private string _licensePlate = string.Empty;
    private string _transporter = string.Empty;
    private string _grossWeightText = string.Empty;
    private string _tareWeightText = string.Empty;
    private string _netWeightText = string.Empty;
    private string _discountText = string.Empty;
    private string _pricePerKgText = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;
    private bool _isLoadingCatalogs;

    // Se activa tras el primer intento de guardar; desde ahi se valida al escribir
    private bool _validateOnChange;

    // Constructor

    public CorrectTicketViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger<CorrectTicketViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    // Eventos

    // Avisa a la ventana que la correccion se guardo para que se cierre
    public event EventHandler? Saved;

    // Propiedades

    public string Description =>
        _ticket is null
            ? string.Empty
            : $"Folio {_ticket.TicketNumber} · {TicketStatusDisplay.WeighingTypeText(_ticket.WeighingType).ToLowerInvariant()}";

    public string StatusText =>
        _ticket is null ? string.Empty : TicketStatusDisplay.Text(_ticket.Status);

    public StatusKind StatusKind =>
        _ticket is null ? StatusKind.Neutral : TicketStatusDisplay.Kind(_ticket.Status);

    // Que se puede corregir en cada estado (RN-15, RN-21, RN-22)
    public bool CanEditGeneral =>
        Status is TicketStatus.WeighingPending or TicketStatus.Completed
        || (Status == TicketStatus.AmountPending && !IsConventional);

    public bool CanEditGrossWeight =>
        IsConventional && Status is TicketStatus.WeighingPending or TicketStatus.Completed;

    public bool CanEditTareWeight =>
        IsConventional && Status is TicketStatus.AmountPending or TicketStatus.Completed;

    public bool CanEditNetWeight =>
        !IsConventional && Status is TicketStatus.AmountPending or TicketStatus.Completed;

    // El descuento y el precio solo existen en un completado; antes se registran en Importes
    public bool CanEditAmount => Status == TicketStatus.Completed;

    public bool ShowsWeightsSection =>
        CanEditGrossWeight || CanEditTareWeight || CanEditNetWeight || CanEditAmount;

    public string WeightsSectionTitle => CanEditAmount ? "PESOS Y PRECIO" : "PESO";

    // Aviso de que se puede corregir en este estado
    public string ScopeNote => Status switch
    {
        TicketStatus.WeighingPending =>
            "El vehículo sigue en planta: se corrigen los datos de la operación y el peso bruto. Los pesos se capturan a mano.",
        TicketStatus.AmountPending when IsConventional =>
            "Pendiente de importe: solo se corrige la tara. El descuento y el precio se registran en Importes.",
        TicketStatus.AmountPending =>
            "Pendiente de importe: se corrigen los datos de la operación y el peso neto. El descuento y el precio se registran en Importes.",
        _ =>
            "Los pesos se capturan a mano. El importe se recalcula con los valores nuevos."
    };

    public ObservableCollection<SupplierOptionDto> Suppliers { get; } = new();

    public ObservableCollection<MaterialOptionDto> Materials { get; } = new();

    public int? SelectedSupplierId
    {
        get => _selectedSupplierId;
        set => SetField(ref _selectedSupplierId, value, ValidateSupplier);
    }

    public int? SelectedMaterialId
    {
        get => _selectedMaterialId;
        set => SetField(ref _selectedMaterialId, value, ValidateMaterial);
    }

    public string LicensePlate
    {
        get => _licensePlate;
        set => SetField(ref _licensePlate, value, ValidateLicensePlate);
    }

    public string Transporter
    {
        get => _transporter;
        set => SetField(ref _transporter, value, ValidateTransporter);
    }

    public string GrossWeightText
    {
        get => _grossWeightText;
        set => SetField(ref _grossWeightText, value, ValidateWeights);
    }

    public string TareWeightText
    {
        get => _tareWeightText;
        set => SetField(ref _tareWeightText, value, ValidateWeights);
    }

    public string NetWeightText
    {
        get => _netWeightText;
        set => SetField(ref _netWeightText, value, ValidateWeights);
    }

    public string DiscountText
    {
        get => _discountText;
        set => SetField(ref _discountText, value, ValidateDiscount);
    }

    public string PricePerKgText
    {
        get => _pricePerKgText;
        set => SetField(ref _pricePerKgText, value, ValidatePricePerKg);
    }

    // "Sin cambio" o "Antes 5,000" debajo de cada campo de peso y precio
    public string GrossWeightHint => DescribeChange(ParseWeight(GrossWeightText), _ticket?.GrossWeight, FormatWeight);

    public string TareWeightHint => DescribeChange(ParseWeight(TareWeightText), _ticket?.TareWeight, FormatWeight);

    public string NetWeightHint => DescribeChange(ParseWeight(NetWeightText), _ticket?.NetWeight, FormatWeight);

    public string DiscountHint =>
        DescribeChange(AmountInput.ParseDiscount(DiscountText), _ticket?.Discount, value => value.ToString(CultureInfo.CurrentCulture));

    public string PricePerKgHint => DescribeChange(AmountInput.ParsePricePerKg(PricePerKgText), _ticket?.PricePerKg, DisplayFormat.Money);

    // El calculo con los valores capturados; un dato invalido se muestra como "—"
    public bool ShowsBreakdown => CanEditTareWeight || CanEditAmount;

    public IReadOnlyList<BreakdownStep> Breakdown
    {
        get
        {
            if (_ticket is null)
                return [];

            decimal? netWeight = EffectiveNetWeight;
            var steps = new List<BreakdownStep>();

            if (IsConventional)
            {
                steps.Add(new("Peso bruto − tara = peso neto", FormatKgOrDash(netWeight), IsResult: !CanEditAmount));
            }
            else
            {
                steps.Add(new("Peso neto", FormatKgOrDash(netWeight)));
            }

            if (!CanEditAmount)
                return steps;

            int? discount = ValidDiscount;
            decimal? pricePerKg = ValidPricePerKg;

            var breakdown = netWeight is decimal net && discount is int d && pricePerKg is decimal p
                ? Ticket.CalculateAmountBreakdown(net, d, p)
                : null;

            steps.Add(new(
                discount is int shown ? $"− Descuento ({shown} %)" : "− Descuento",
                breakdown is null ? "—" : DisplayFormat.DecimalKg(breakdown.DiscountWeight)));

            steps.Add(new(
                "= Peso neto total",
                breakdown is null ? "—" : DisplayFormat.DecimalKg(breakdown.NetWeightAfterDiscount)));

            steps.Add(new(
                "× Precio por kg",
                pricePerKg is decimal price ? DisplayFormat.Money(price) : "—"));

            steps.Add(new(
                "= Importe nuevo",
                breakdown is null ? "—" : DisplayFormat.Money(breakdown.Amount),
                IsResult: true));

            return steps;
        }
    }

    // Importe guardado y la diferencia contra el nuevo, solo en un completado
    public string AmountComparison
    {
        get
        {
            if (!CanEditAmount || _ticket?.Amount is not decimal saved)
                return string.Empty;

            decimal? newAmount = NewAmount;

            if (newAmount is not decimal value)
                return $"Importe guardado: {DisplayFormat.Money(saved)}";

            decimal difference = value - saved;

            return difference == 0
                ? $"Importe guardado: {DisplayFormat.Money(saved)} · sin cambio"
                : $"Importe guardado: {DisplayFormat.Money(saved)} · diferencia {(difference > 0 ? "+" : "−")}{DisplayFormat.Money(Math.Abs(difference))}";
        }
    }

    public bool ShowsAmountComparison => CanEditAmount;

    // Errores que no pertenecen a un campo, como un ticket que cambio de estado
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
                SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsLoadingCatalogs
    {
        get => _isLoadingCatalogs;
        private set
        {
            if (SetProperty(ref _isLoadingCatalogs, value))
                SaveCommand.NotifyCanExecuteChanged();
        }
    }

    // Comandos

    public AsyncRelayCommand SaveCommand { get; }

    // Propiedades privadas

    private TicketStatus? Status => _ticket?.Status;

    private bool IsConventional => _ticket?.WeighingType == WeighingType.Conventional;

    // Bruto − tara en el convencional; el capturado en el pesaje unico
    private decimal? EffectiveNetWeight =>
        IsConventional
            ? (ParseWeight(GrossWeightText) is decimal gross && ParseWeight(TareWeightText) is decimal tare && tare < gross
                ? gross - tare
                : null)
            : ParseWeight(NetWeightText);

    private int? ValidDiscount => AmountInput.ValidDiscount(DiscountText);

    private decimal? ValidPricePerKg => AmountInput.ValidPricePerKg(PricePerKgText);

    private decimal? NewAmount =>
        EffectiveNetWeight is decimal net && ValidDiscount is int discount && ValidPricePerKg is decimal price
            ? Ticket.CalculateAmountBreakdown(net, discount, price).Amount
            : null;

    // Metodos publicos

    // Los campos empiezan con lo guardado; los que no se pueden corregir no se envian
    public void Initialize(PurchaseTicketDetail ticket)
    {
        _ticket = ticket;

        _selectedSupplierId = ticket.SupplierId;
        _selectedMaterialId = ticket.MaterialId;
        _licensePlate = ticket.LicensePlate ?? string.Empty;
        _transporter = ticket.Transporter;
        _grossWeightText = FormatWeightInput(ticket.GrossWeight);
        _tareWeightText = FormatWeightInput(ticket.TareWeight);
        _netWeightText = FormatWeightInput(ticket.NetWeight);
        _discountText = ticket.Discount?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        _pricePerKgText = AmountInput.FormatPriceInput(ticket.PricePerKg);

        _validateOnChange = false;
        ErrorMessage = string.Empty;
        ClearErrors();

        // Todas las propiedades dependen del ticket: se avisan juntas
        OnPropertyChanged(string.Empty);
    }

    // Solo hace falta si se pueden corregir proveedor y material
    public async Task LoadCatalogsAsync()
    {
        if (_ticket is null || !CanEditGeneral)
            return;

        IsLoadingCatalogs = true;

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

            // Si el actual se desactivo no viene en los activos; se agrega para que
            // se vea. Mientras no se cambie no se envia y no se vuelve a validar
            if (Suppliers.All(supplier => supplier.Id != _ticket.SupplierId))
                Suppliers.Add(new SupplierOptionDto(_ticket.SupplierId, $"{_ticket.SupplierName} (inactivo)"));

            if (Materials.All(material => material.Id != _ticket.MaterialId))
                Materials.Add(new MaterialOptionDto(_ticket.MaterialId, $"{_ticket.MaterialName} (inactivo)"));

            OnPropertyChanged(nameof(SelectedSupplierId));
            OnPropertyChanged(nameof(SelectedMaterialId));
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible cargar los proveedores y materiales.";

            _logger.LogError(ex, "No fue posible cargar los catálogos para corregir el ticket {TicketId}.", _ticket.Id);
        }
        finally
        {
            IsLoadingCatalogs = false;
        }
    }

    // Metodos privados

    // Solo enteros con separador de miles opcional, igual que la captura manual de Pesaje
    private static decimal? ParseWeight(string text)
    {
        return decimal.TryParse(text.Trim(), NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out decimal weight)
            ? weight
            : null;
    }

    private static string FormatWeightInput(decimal? weight)
    {
        return weight?.ToString("N0", CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private static string FormatWeight(decimal weight)
    {
        return weight.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string FormatKgOrDash(decimal? weight)
    {
        return weight is decimal value ? DisplayFormat.DecimalKg(value) : "—";
    }

    private static string DescribeChange<T>(T? current, T? original, Func<T, string> format)
        where T : struct
    {
        if (original is not T saved)
            return string.Empty;

        return Equals(current, saved) ? "Sin cambio" : $"Antes {format(saved)}";
    }

    // El valor nuevo solo si el campo se puede corregir y es distinto de lo guardado
    private static T? Changed<T>(bool canEdit, T? current, T? original)
        where T : struct
    {
        return canEdit && current is T value && !Equals(value, original)
            ? value
            : null;
    }

    private static string? CheckWeight(string text, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(text))
            return $"{fieldName} es obligatorio.";

        if (ParseWeight(text) is not decimal weight)
            return "Escribe el peso en kilogramos enteros, sin decimales.";

        return weight <= 0 ? $"{fieldName} debe ser mayor a cero." : null;
    }

    // Comun a todos los campos: avisa al calculo y valida si ya se intento guardar
    private void SetField<T>(
        ref T field,
        T value,
        Func<bool> validate,
        [CallerMemberName] string propertyName = "")
    {
        if (!SetProperty(ref field, value, propertyName))
            return;

        OnPropertyChanged(nameof(Breakdown));
        OnPropertyChanged(nameof(AmountComparison));
        OnPropertyChanged(nameof(GrossWeightHint));
        OnPropertyChanged(nameof(TareWeightHint));
        OnPropertyChanged(nameof(NetWeightHint));
        OnPropertyChanged(nameof(DiscountHint));
        OnPropertyChanged(nameof(PricePerKgHint));

        if (_validateOnChange)
            validate();
    }

    private bool CanSave()
    {
        return _ticket is not null && !IsSaving && !IsLoadingCatalogs;
    }

    private async Task SaveAsync()
    {
        if (!CanSave())
            return;

        ErrorMessage = string.Empty;
        _validateOnChange = true;

        if (!ValidateAll())
            return;

        var command = BuildCommand(_ticket!);

        if (command is null)
        {
            ErrorMessage = "No hay cambios que guardar.";
            return;
        }

        IsSaving = true;

        bool saved = false;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            await scope.ServiceProvider
                .GetRequiredService<CorrectPurchaseTicket>()
                .ExecuteAsync(command);

            saved = true;
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            // Por ejemplo, otro equipo cambio el estado del ticket mientras tanto
            ErrorMessage = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = GetUserMessage(ex);
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible guardar la corrección.";

            _logger.LogError(ex, "No fue posible corregir el ticket {TicketId}.", _ticket!.Id);
        }
        finally
        {
            IsSaving = false;
        }

        // Fuera del try: un error al cerrar la ventana no es un error al guardar
        if (saved)
            Saved?.Invoke(this, EventArgs.Empty);
    }

    // Null si nada cambio. Placas y chofer se comparan ya normalizados:
    // "abc123" y "ABC123" son el mismo valor guardado
    private CorrectPurchaseTicketCommand? BuildCommand(PurchaseTicketDetail ticket)
    {
        int? supplierId = CanEditGeneral && SelectedSupplierId != ticket.SupplierId ? SelectedSupplierId : null;
        int? materialId = CanEditGeneral && SelectedMaterialId != ticket.MaterialId ? SelectedMaterialId : null;

        string? licensePlate = IsConventional
            ? Ticket.NormalizeLicensePlate(LicensePlate)
            : Ticket.NormalizeOptionalLicensePlate(LicensePlate);

        bool changeLicensePlate = CanEditGeneral && licensePlate != ticket.LicensePlate;

        string? transporter =
            CanEditGeneral && Ticket.NormalizeTransporter(Transporter) != ticket.Transporter
                ? Transporter
                : null;

        decimal? gross = Changed(CanEditGrossWeight, ParseWeight(GrossWeightText), ticket.GrossWeight);
        decimal? tare = Changed(CanEditTareWeight, ParseWeight(TareWeightText), ticket.TareWeight);
        decimal? net = Changed(CanEditNetWeight, ParseWeight(NetWeightText), ticket.NetWeight);
        int? discount = Changed(CanEditAmount, AmountInput.ParseDiscount(DiscountText), ticket.Discount);
        decimal? price = Changed(CanEditAmount, AmountInput.ParsePricePerKg(PricePerKgText), ticket.PricePerKg);

        bool hasChanges =
            supplierId is not null || materialId is not null || changeLicensePlate || transporter is not null
            || gross is not null || tare is not null || net is not null || discount is not null || price is not null;

        if (!hasChanges)
            return null;

        return new CorrectPurchaseTicketCommand(
            TicketId: ticket.Id,
            SupplierId: supplierId,
            MaterialId: materialId,
            Transporter: transporter,
            ChangeLicensePlate: changeLicensePlate,
            LicensePlate: changeLicensePlate ? licensePlate : null,
            GrossWeight: gross,
            TareWeight: tare,
            NetWeight: net,
            Discount: discount,
            PricePerKg: price);
    }

    private bool ValidateAll()
    {
        // Se validan todos para mostrar todos los errores a la vez
        bool[] results =
        [
            ValidateSupplier(),
            ValidateMaterial(),
            ValidateLicensePlate(),
            ValidateTransporter(),
            ValidateWeights(),
            ValidateDiscount(),
            ValidatePricePerKg()
        ];

        return results.All(isValid => isValid);
    }

    private bool ValidateSupplier()
    {
        string? error = CanEditGeneral && SelectedSupplierId is null ? "Selecciona un proveedor." : null;

        return Apply(nameof(SelectedSupplierId), error);
    }

    private bool ValidateMaterial()
    {
        string? error = CanEditGeneral && SelectedMaterialId is null ? "Selecciona un material." : null;

        return Apply(nameof(SelectedMaterialId), error);
    }

    // Mismas reglas del dominio: obligatorias en el convencional, opcionales en el unico
    private bool ValidateLicensePlate()
    {
        string? error = !CanEditGeneral ? null
            : IsConventional ? ValidateRule(() => Ticket.NormalizeLicensePlate(LicensePlate))
            : ValidateRule(() => Ticket.NormalizeOptionalLicensePlate(LicensePlate));

        return Apply(nameof(LicensePlate), error);
    }

    private bool ValidateTransporter()
    {
        string? error = CanEditGeneral ? ValidateRule(() => Ticket.NormalizeTransporter(Transporter)) : null;

        return Apply(nameof(Transporter), error);
    }

    // Los tres pesos juntos: la tara depende del bruto (RN-35 a RN-37)
    private bool ValidateWeights()
    {
        string? grossError = CanEditGrossWeight ? CheckWeight(GrossWeightText, "El peso bruto") : null;
        string? tareError = CanEditTareWeight ? CheckWeight(TareWeightText, "La tara") : null;
        string? netError = CanEditNetWeight ? CheckWeight(NetWeightText, "El peso neto") : null;

        if (tareError is null
            && IsConventional
            && CanEditTareWeight
            && ParseWeight(TareWeightText) is decimal tare
            && ParseWeight(GrossWeightText) is decimal gross
            && tare >= gross)
        {
            tareError = "La tara debe ser menor al peso bruto.";
        }

        bool isGrossValid = Apply(nameof(GrossWeightText), grossError);
        bool isTareValid = Apply(nameof(TareWeightText), tareError);
        bool isNetValid = Apply(nameof(NetWeightText), netError);

        return isGrossValid && isTareValid && isNetValid;
    }

    private bool ValidateDiscount()
    {
        string? error = CanEditAmount ? AmountInput.DiscountError(DiscountText) : null;

        return Apply(nameof(DiscountText), error);
    }

    private bool ValidatePricePerKg()
    {
        string? error = CanEditAmount ? AmountInput.PricePerKgError(PricePerKgText) : null;

        return Apply(nameof(PricePerKgText), error);
    }

    private bool Apply(string propertyName, string? error)
    {
        SetError(propertyName, error);

        return error is null;
    }
}