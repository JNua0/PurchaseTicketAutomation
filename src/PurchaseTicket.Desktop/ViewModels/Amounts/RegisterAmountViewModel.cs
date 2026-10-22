using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterAmount;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Forms;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Exceptions;
using PurchaseTicket.Domain.ValueObjects;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Desktop.ViewModels.Amounts;

// Registrar importe (CU-04): descuento y precio de un ticket pendiente de
// importe, con el calculo en vivo. El calculo y las reglas son los del
// dominio, asi lo que se ve es exactamente lo que se guarda
public class RegisterAmountViewModel : ValidatableObject
{
    // Campos

    // Permite escribir "$3.50" o "1,250.00"; no permite negativos ni exponentes
    private const NumberStyles PriceStyles =
        NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowTrailingWhite
        | NumberStyles.AllowCurrencySymbol
        | NumberStyles.AllowThousands
        | NumberStyles.AllowDecimalPoint;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RegisterAmountViewModel> _logger;
    private AmountListItem? _ticket;
    private IReadOnlyList<DetailItem> _details = [];
    private string _discountText = string.Empty;
    private string _pricePerKgText = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;

    // Se activa tras el primer intento de guardar; desde ahi se valida al escribir
    private bool _validateOnChange;

    // Constructor

    public RegisterAmountViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger<RegisterAmountViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    // Eventos

    // Avisa a la ventana que el importe se registro para que se cierre
    public event EventHandler? Registered;

    // Propiedades

    public string Description =>
        $"Descuento y precio del ticket folio {_ticket?.TicketNumber}";

    // Datos del ticket de solo lectura; sin bruto ni tara
    public IReadOnlyList<DetailItem> Details
    {
        get => _details;
        private set => SetProperty(ref _details, value);
    }

    // Sin valor inicial: el descuento se escribe siempre, aunque sea 0
    public string DiscountText
    {
        get => _discountText;
        set
        {
            if (!SetProperty(ref _discountText, value))
                return;

            OnPropertyChanged(nameof(Breakdown));

            if (_validateOnChange)
                ValidateDiscount();
        }
    }

    public string PricePerKgText
    {
        get => _pricePerKgText;
        set
        {
            if (!SetProperty(ref _pricePerKgText, value))
                return;

            OnPropertyChanged(nameof(Breakdown));

            if (_validateOnChange)
                ValidatePricePerKg();
        }
    }

    // Peso neto − descuento = peso neto total × precio = importe. Un dato que
    // falta o no cumple su regla se muestra como "—"
    public IReadOnlyList<BreakdownStep> Breakdown
    {
        get
        {
            decimal netWeight = NetWeight;
            int? discount = ValidDiscount;
            decimal? pricePerKg = ValidPricePerKg;

            // El descuento en kg solo depende del descuento: el precio de 1 es
            // solo para reutilizar el calculo del dominio
            AmountBreakdown? discountOnly = discount is int value
                ? Ticket.CalculateAmountBreakdown(netWeight, value, 1m)
                : null;

            AmountBreakdown? full = discount is int d && pricePerKg is decimal p
                ? Ticket.CalculateAmountBreakdown(netWeight, d, p)
                : null;

            return
            [
                new("Peso neto", FormatKg(netWeight)),
                new(
                    discount is int shown ? $"− Descuento ({shown} %)" : "− Descuento",
                    discountOnly is null ? "—" : FormatKg(discountOnly.DiscountWeight)),
                new(
                    "= Peso neto total",
                    discountOnly is null ? "—" : FormatKg(discountOnly.NetWeightAfterDiscount)),
                new(
                    "× Precio por kg",
                    pricePerKg is decimal price ? FormatMoney(price) : "—"),
                new(
                    "= Importe",
                    full is null ? "—" : FormatMoney(full.Amount),
                    IsResult: true)
            ];
        }
    }

    // Errores que no pertenecen a un campo, como un ticket que ya no esta pendiente
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

    // Resultado del registro; el servicio de dialogo lo lee al cerrar
    public AmountRegistration? Registration { get; private set; }

    // Comandos

    public AsyncRelayCommand SaveCommand { get; }

    // Propiedades privadas

    // Un ticket pendiente de importe siempre tiene peso neto
    private decimal NetWeight => _ticket?.NetWeight ?? 0;

    // El valor si se puede leer y cumple la regla; si no, null
    private int? ValidDiscount =>
        TryParseDiscount(out int discount)
        && ValidateRule(() => Ticket.ValidateDiscount(discount)) is null
            ? discount
            : null;

    private decimal? ValidPricePerKg =>
        TryParsePricePerKg(out decimal pricePerKg)
        && ValidateRule(() => Ticket.ValidatePricePerKg(pricePerKg)) is null
            ? pricePerKg
            : null;

    // Metodos publicos

    // Los datos vienen de la fila de la lista: no hace falta otra consulta
    public void Initialize(AmountListItem ticket)
    {
        _ticket = ticket;

        Details =
        [
            new("Proveedor", ticket.SupplierName),
            new("Material", ticket.MaterialName),
            new("Modalidad", ticket.WeighingTypeText),
            new("Pesaje terminado", ticket.WeighedAt.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture))
        ];

        DiscountText = string.Empty;
        PricePerKgText = string.Empty;

        _validateOnChange = false;
        ErrorMessage = string.Empty;
        ClearErrors();

        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(Breakdown));
    }

    // Metodos privados

    private static string FormatKg(decimal weight)
    {
        return $"{weight.ToString("N2", CultureInfo.CurrentCulture)} kg";
    }

    private static string FormatMoney(decimal amount)
    {
        return $"${amount.ToString("N2", CultureInfo.CurrentCulture)}";
    }

    private bool TryParseDiscount(out int discount)
    {
        // Solo digitos: sin signo, decimales ni separadores (RN-38)
        return int.TryParse(
            DiscountText.Trim(),
            NumberStyles.None,
            CultureInfo.CurrentCulture,
            out discount);
    }

    private bool TryParsePricePerKg(out decimal pricePerKg)
    {
        return decimal.TryParse(
            PricePerKgText,
            PriceStyles,
            CultureInfo.CurrentCulture,
            out pricePerKg);
    }

    private bool CanSave()
    {
        return _ticket is not null && !IsSaving;
    }

    private async Task SaveAsync()
    {
        if (!CanSave())
            return;

        ErrorMessage = string.Empty;
        _validateOnChange = true;

        // Las reglas se revisan antes de ir a la base de datos
        if (!ValidateAll())
            return;

        TryParseDiscount(out int discount);
        TryParsePricePerKg(out decimal pricePerKg);

        IsSaving = true;

        bool registered = false;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            await scope.ServiceProvider
                .GetRequiredService<RegisterAmount>()
                .ExecuteAsync(new RegisterAmountCommand(
                    _ticket!.Id,
                    discount,
                    pricePerKg));

            var breakdown = Ticket.CalculateAmountBreakdown(
                NetWeight,
                discount,
                pricePerKg);

            Registration = new AmountRegistration(
                _ticket.TicketNumber,
                breakdown.Amount);

            registered = true;
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            // Por ejemplo, otro equipo ya registro el importe o cancelo el ticket
            ErrorMessage = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = GetUserMessage(ex);
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible registrar el importe.";

            _logger.LogError(
                ex,
                "No fue posible registrar el importe del ticket {TicketId}.",
                _ticket!.Id);
        }
        finally
        {
            IsSaving = false;
        }

        // Fuera del try: un error al cerrar la ventana no debe
        // mostrarse como error al registrar
        if (registered)
            Registered?.Invoke(this, EventArgs.Empty);
    }

    private bool ValidateAll()
    {
        bool isDiscountValid = ValidateDiscount();
        bool isPricePerKgValid = ValidatePricePerKg();

        return isDiscountValid && isPricePerKgValid;
    }

    private bool ValidateDiscount()
    {
        string? error =
            string.IsNullOrWhiteSpace(DiscountText) ? "El descuento es obligatorio."
            : !TryParseDiscount(out int discount) ? "El descuento debe ser un número entero entre 0 y 99."
            : ValidateRule(() => Ticket.ValidateDiscount(discount));

        SetError(nameof(DiscountText), error);

        return error is null;
    }

    private bool ValidatePricePerKg()
    {
        string? error =
            string.IsNullOrWhiteSpace(PricePerKgText) ? "El precio por kilogramo es obligatorio."
            : !TryParsePricePerKg(out decimal pricePerKg) ? "Escribe el precio como un número, por ejemplo 3.50."
            : ValidateRule(() => Ticket.ValidatePricePerKg(pricePerKg));

        SetError(nameof(PricePerKgText), error);

        return error is null;
    }
}