using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.GetDetail;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Detalle del ticket (CU-05): toda su informacion de solo lectura y las
// operaciones que permite su estado. Cancelado solo se consulta
public sealed class TicketDetailViewModel : ObservableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITicketActionsService _ticketActions;
    private readonly ITicketsDialogService _dialogs;
    private readonly INotificationService _notifications;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TicketDetailViewModel> _logger;

    private int _ticketId;
    private PurchaseTicketDetail? _detail;
    private bool _isLoading;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    // Constructor

    public TicketDetailViewModel(
        IServiceScopeFactory scopeFactory,
        ITicketActionsService ticketActions,
        ITicketsDialogService dialogs,
        INotificationService notifications,
        TimeProvider timeProvider,
        ILogger<TicketDetailViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _ticketActions = ticketActions;
        _dialogs = dialogs;
        _notifications = notifications;
        _timeProvider = timeProvider;
        _logger = logger;

        ReprintCommand = new AsyncRelayCommand(ReprintAsync, () => CanReprint && !IsBusy);
        CancelTicketCommand = new AsyncRelayCommand(CancelTicketAsync, () => CanCancel && !IsBusy);
        CorrectCommand = new AsyncRelayCommand(CorrectAsync, () => CanCorrect && !IsBusy);
    }

    // Propiedades

    public string Title =>
        _detail is null ? "Detalle del ticket" : $"Ticket folio {_detail.TicketNumber}";

    // Modalidad y la fecha de la ultima modificacion, o de registro si nunca se modifico
    public string Description
    {
        get
        {
            if (_detail is null)
                return string.Empty;

            string modality = _detail.WeighingType == WeighingType.Conventional
                ? "Pesaje convencional"
                : "Un solo pesaje";

            return _detail.UpdatedAt is DateTime updated
                ? $"{modality} · última modificación {FormatDateTime(updated)}"
                : $"{modality} · registrado {FormatDateTime(_detail.CheckInAt)}";
        }
    }

    public string StatusText =>
        _detail is null ? string.Empty : TicketStatusDisplay.Text(_detail.Status);

    public StatusKind StatusKind =>
        _detail is null ? StatusKind.Neutral : TicketStatusDisplay.Kind(_detail.Status);

    public bool HasDetail => _detail is not null;

    // Cada dato por separado: la ventana los acomoda por bloque, con el
    // proveedor y el peso neto a doble ancho, para que las filas se llenen
    public string SupplierName => _detail?.SupplierName ?? string.Empty;

    public string MaterialName => _detail?.MaterialName ?? string.Empty;

    public string WeighingTypeText =>
        _detail is null ? string.Empty : TicketStatusDisplay.WeighingTypeText(_detail.WeighingType);

    public string LicensePlateText => _detail?.LicensePlate ?? "Sin placas";

    public string Transporter => _detail?.Transporter ?? string.Empty;

    public bool IsConventional => _detail?.WeighingType == WeighingType.Conventional;

    public bool IsSingle => _detail?.WeighingType == WeighingType.Single;

    public string GrossWeightText => FormatKg(_detail?.GrossWeight);

    public string TareWeightText => FormatKg(_detail?.TareWeight);

    public string NetWeightText => FormatKg(_detail?.NetWeight);

    // Entrada en el convencional; la unica medicion en el pesaje unico
    public string CheckInText =>
        _detail is null ? string.Empty : FormatDateTime(_detail.CheckInAt);

    public string DepartureText =>
        _detail?.DepartureAt is DateTime departure ? FormatDateTime(departure) : "Pendiente";

    // Solo los completados tienen importe
    public bool HasAmount => _detail?.Amount is not null;

    public string DiscountText =>
        _detail?.Discount is int discount && _detail.DiscountWeight is decimal weight
            ? $"{discount} % · {FormatKgWithDecimals(weight)}"
            : string.Empty;

    public string NetWeightAfterDiscountText =>
        _detail?.NetWeightAfterDiscount is decimal weight ? FormatKgWithDecimals(weight) : string.Empty;

    public string PricePerKgText =>
        _detail?.PricePerKg is decimal price ? FormatMoney(price) : string.Empty;

    // Sin importe: aun no se registra, o ya no se registrara
    public string AmountPendingText =>
        _detail?.Status == TicketStatus.Cancelled
            ? "No aplica: el ticket está cancelado."
            : "Pendiente: se registra en Importes.";

    public string AmountText =>
        _detail?.Amount is decimal amount ? FormatMoney(amount) : string.Empty;

    // Cancelado no se imprime (RN-28); Completado no se cancela (RN-26)
    public bool CanReprint =>
        _detail is not null && _detail.Status != TicketStatus.Cancelled;

    // Cancelado no se corrige (RN-28); en el resto, la ventana decide que campos
    public bool CanCorrect =>
        _detail is not null && _detail.Status != TicketStatus.Cancelled;

    public bool CanCancel =>
        _detail is not null
        && _detail.Status is TicketStatus.WeighingPending or TicketStatus.AmountPending;

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    // Mientras imprime o cancela no se permite otra operacion
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
                NotifyCommands();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    // La lista se recarga al cerrar si el ticket cambio, por ejemplo al cancelarlo
    public bool HasChanges { get; private set; }

    // Comandos

    public AsyncRelayCommand ReprintCommand { get; }

    public AsyncRelayCommand CancelTicketCommand { get; }

    public AsyncRelayCommand CorrectCommand { get; }

    // Metodos publicos

    public void Initialize(int ticketId)
    {
        _ticketId = ticketId;
    }

    // La ventana lo llama al abrirse, y de nuevo despues de cancelar
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var detail = await scope.ServiceProvider
                .GetRequiredService<GetPurchaseTicketDetail>()
                .ExecuteAsync(_ticketId);

            SetDetail(detail);
        }
        catch (NotFoundException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible cargar el ticket.";

            _logger.LogError(ex, "No fue posible cargar el detalle del ticket {TicketId}.", _ticketId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Metodos privados

    private static string FormatKg(decimal? weight)
    {
        return weight is decimal value
            ? $"{value.ToString("N0", CultureInfo.CurrentCulture)} kg"
            : "Pendiente";
    }

    private static string FormatKgWithDecimals(decimal value)
    {
        return $"{value.ToString("N2", CultureInfo.CurrentCulture)} kg";
    }

    private static string FormatMoney(decimal amount)
    {
        return $"${amount.ToString("N2", CultureInfo.CurrentCulture)}";
    }

    private string FormatDateTime(DateTime utc)
    {
        // Npgsql ya devuelve UTC; esto solo protege de un DateTime sin tipo
        if (utc.Kind != DateTimeKind.Utc)
            utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

        return TimeZoneInfo
            .ConvertTimeFromUtc(utc, _timeProvider.LocalTimeZone)
            .ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
    }

    private void SetDetail(PurchaseTicketDetail detail)
    {
        _detail = detail;

        // Todas las propiedades se calculan del detalle: se avisan juntas
        OnPropertyChanged(string.Empty);
        NotifyCommands();
    }

    private async Task ReprintAsync()
    {
        if (_detail is null)
            return;

        IsBusy = true;

        try
        {
            await _ticketActions.ReprintAsync(_detail.Id, _detail.TicketNumber);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelTicketAsync()
    {
        if (_detail is null)
            return;

        CancelTicketOutcome outcome;

        IsBusy = true;

        try
        {
            outcome = await _ticketActions.CancelAsync(_detail.Id, _detail.TicketNumber);
        }
        finally
        {
            IsBusy = false;
        }

        // Cancelado, o rechazado porque otro equipo ya lo cambio: se muestra como quedo
        if (outcome is CancelTicketOutcome.Cancelled or CancelTicketOutcome.Rejected)
        {
            HasChanges = true;
            await LoadAsync();
        }
    }

    // RN-30: la correccion no reimprime; el operador decide si reimprimir despues
    private async Task CorrectAsync()
    {
        if (_detail is null || !_dialogs.ShowCorrectDialog(_detail))
            return;

        _notifications.ShowSuccess($"Ticket folio {_detail.TicketNumber} corregido.");

        HasChanges = true;
        await LoadAsync();
    }

    private void NotifyCommands()
    {
        ReprintCommand?.NotifyCanExecuteChanged();
        CancelTicketCommand?.NotifyCanExecuteChanged();
        CorrectCommand?.NotifyCanExecuteChanged();
    }
}