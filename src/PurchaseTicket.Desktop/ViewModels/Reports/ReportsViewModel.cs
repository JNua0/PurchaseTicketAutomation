using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Reports;
using PurchaseTicket.Application.UseCases.Reports.Purchases;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Desktop.ViewModels.Tickets;

namespace PurchaseTicket.Desktop.ViewModels.Reports;

// Reportes (CU-22): kilos comprados en un periodo, por material y por
// proveedor. Cuentan completados y pendientes de importe; el importe, solo
// los completados. Se exporta a Excel y a PDF
public sealed class ReportsViewModel : ObservableObject
{
    // Campos

    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-MX");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPurchaseReportExporter _exporter;
    private readonly IReportsDialogService _dialogs;
    private readonly INotificationService _notifications;
    private readonly ApplicationSettingsService _settings;
    private readonly TimeProvider _clock;
    private readonly ILogger<ReportsViewModel> _logger;

    private FilterOption<TicketPeriod> _selectedPeriod;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private DateTime? _startLocal;
    private DateTime? _endLocal;
    private PurchaseReport? _report;
    private IReadOnlyList<ReportGroupRow> _materials = [];
    private IReadOnlyList<ReportGroupRow> _suppliers = [];
    private bool _isLoading;
    private bool _isExporting;
    private string _errorMessage = string.Empty;
    private string _dateRangeError = string.Empty;

    // Cuenta las cargas: si el operador cambia de periodo rapido, solo vale la ultima
    private int _loadVersion;

    // Constructor

    public ReportsViewModel(
        IServiceScopeFactory scopeFactory,
        IPurchaseReportExporter exporter,
        IReportsDialogService dialogs,
        INotificationService notifications,
        ApplicationSettingsService settings,
        TimeProvider clock,
        ILogger<ReportsViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _exporter = exporter;
        _dialogs = dialogs;
        _notifications = notifications;
        _settings = settings;
        _clock = clock;
        _logger = logger;

        // Este mes por defecto: es lo que mas se consulta
        _selectedPeriod = PeriodOptions[3];

        ExportExcelCommand = new AsyncRelayCommand(() => ExportAsync("xlsx"), CanExport);
        ExportPdfCommand = new AsyncRelayCommand(() => ExportAsync("pdf"), CanExport);
    }

    // Propiedades

    // Los mismos periodos que Tickets, sin "Todos": un reporte siempre tiene fechas
    public IReadOnlyList<FilterOption<TicketPeriod>> PeriodOptions { get; } =
    [
        new(TicketPeriod.Today, "Hoy"),
        new(TicketPeriod.Yesterday, "Ayer"),
        new(TicketPeriod.Last7Days, "Últimos 7 días"),
        new(TicketPeriod.ThisMonth, "Este mes"),
        new(TicketPeriod.LastMonth, "Mes anterior"),
        new(TicketPeriod.Custom, "Personalizado")
    ];

    public FilterOption<TicketPeriod> SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (value is null || !SetProperty(ref _selectedPeriod, value))
                return;

            OnPropertyChanged(nameof(IsCustomPeriod));
            _ = LoadAsync();
        }
    }

    // Desde y Hasta solo se muestran con un periodo personalizado
    public bool IsCustomPeriod => SelectedPeriod.Value == TicketPeriod.Custom;

    public DateTime? FromDate
    {
        get => _fromDate;
        set
        {
            if (SetProperty(ref _fromDate, value))
                _ = LoadAsync();
        }
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set
        {
            if (SetProperty(ref _toDate, value))
                _ = LoadAsync();
        }
    }

    public string DateRangeError
    {
        get => _dateRangeError;
        private set => SetProperty(ref _dateRangeError, value);
    }

    // "1 al 23 de octubre de 2026"
    public string RangeText =>
        _startLocal is DateTime start && _endLocal is DateTime end
            ? DescribeRange(start, end.AddDays(-1))
            : "Elige las fechas Desde y Hasta";

    public bool HasReport => _report is not null;

    public bool IsEmpty => _report is { IsEmpty: true };

    public bool HasData => _report is { IsEmpty: false };

    public string TicketCountText => _report?.TicketCount.ToString("N0", CultureInfo.CurrentCulture) ?? "—";

    public string TicketBreakdownText =>
        _report is null
            ? string.Empty
            : $"{_report.CompletedCount} completados · {_report.PendingAmountCount} pendientes de importe";

    public string NetWeightText => _report is null ? "—" : DisplayFormat.WholeKg(_report.NetWeight);

    public string NetWeightAfterDiscountText => _report is null ? "—" : DisplayFormat.DecimalKg(_report.NetWeightAfterDiscount);

    public string AmountText => _report is null ? "—" : DisplayFormat.Money(_report.Amount);

    public IReadOnlyList<ReportGroupRow> Materials => _materials;

    public IReadOnlyList<ReportGroupRow> Suppliers => _suppliers;

    public string MaterialsCountText => CountText(_materials.Count, "material", "materiales");

    public string SuppliersCountText => CountText(_suppliers.Count, "proveedor", "proveedores");

    public string TotalPendingText =>
        _report is { PendingAmountCount: > 0 } report ? $"{report.PendingAmountCount} sin importe" : string.Empty;

    public bool HasTotalPending => TotalPendingText.Length > 0;

    public string EmptyMessage => "No hay tickets completados ni pendientes de importe en este periodo.";

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsExporting
    {
        get => _isExporting;
        private set
        {
            if (!SetProperty(ref _isExporting, value))
                return;

            ExportExcelCommand.NotifyCanExecuteChanged();
            ExportPdfCommand.NotifyCanExecuteChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    // Comandos

    public AsyncRelayCommand ExportExcelCommand { get; }

    public AsyncRelayCommand ExportPdfCommand { get; }

    // Metodos publicos

    // Al entrar a la pantalla y al cambiar el periodo o las fechas
    public async Task LoadAsync()
    {
        int version = ++_loadVersion;

        ErrorMessage = string.Empty;

        TimeZoneInfo zone = _clock.LocalTimeZone;
        DateTime today = TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), zone).DateTime;

        (_startLocal, _endLocal) = PeriodRange.ToLocal(
            SelectedPeriod.Value ?? TicketPeriod.ThisMonth,
            FromDate,
            ToDate,
            today);

        OnPropertyChanged(nameof(RangeText));

        if (!ValidateRange())
        {
            IsLoading = false;
            SetReport(null);
            return;
        }

        IsLoading = true;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var report = await scope.ServiceProvider
                .GetRequiredService<GetPurchaseReport>()
                .ExecuteAsync(
                    PeriodRange.ToUtc(_startLocal, zone)!.Value,
                    PeriodRange.ToUtc(_endLocal, zone)!.Value);

            if (version == _loadVersion)
                SetReport(report);
        }
        catch (Exception ex)
        {
            if (version != _loadVersion)
                return;

            SetReport(null);
            ErrorMessage = "No fue posible cargar el reporte.";

            _logger.LogError(ex, "No fue posible cargar el reporte de {Start} a {End}.", _startLocal, _endLocal);
        }
        finally
        {
            if (version == _loadVersion)
                IsLoading = false;
        }
    }

    // Metodos privados

    private static string CountText(int count, string singular, string plural)
    {
        return count == 1 ? $"1 {singular} con tickets" : $"{count} {plural} con tickets";
    }

    // "Viernes 23 de octubre de 2026", "1 al 23 de octubre de 2026",
    // "28 de septiembre al 4 de octubre de 2026" o con los dos años
    private static string DescribeRange(DateTime first, DateTime last)
    {
        if (first.Date == last.Date)
        {
            string day = first.ToString("dddd d 'de' MMMM 'de' yyyy", Spanish);

            return char.ToUpper(day[0], Spanish) + day[1..];
        }

        if (first.Year != last.Year)
            return $"{first.ToString("d 'de' MMMM 'de' yyyy", Spanish)} al {last.ToString("d 'de' MMMM 'de' yyyy", Spanish)}";

        if (first.Month != last.Month)
            return $"{first.ToString("d 'de' MMMM", Spanish)} al {last.ToString("d 'de' MMMM 'de' yyyy", Spanish)}";

        return $"{first.Day} al {last.ToString("d 'de' MMMM 'de' yyyy", Spanish)}";
    }

    private bool ValidateRange()
    {
        if (!IsCustomPeriod)
        {
            DateRangeError = string.Empty;
            return true;
        }

        if (FromDate is null || ToDate is null)
        {
            DateRangeError = string.Empty;
            return false;
        }

        bool isValid = FromDate.Value.Date <= ToDate.Value.Date;

        DateRangeError = isValid ? string.Empty : "La fecha inicial no puede ser posterior a la final.";

        return isValid;
    }

    private void SetReport(PurchaseReport? report)
    {
        _report = report;

        _materials = report is null
            ? []
            : report.ByMaterial.Select(group => new ReportGroupRow(group, report.ShareOf(group))).ToList();

        _suppliers = report is null
            ? []
            : report.BySupplier.Select(group => new ReportGroupRow(group, report.ShareOf(group))).ToList();

        // Todas las propiedades del reporte se avisan juntas
        OnPropertyChanged(string.Empty);

        ExportExcelCommand.NotifyCanExecuteChanged();
        ExportPdfCommand.NotifyCanExecuteChanged();
    }

    private bool CanExport()
    {
        return HasData && !IsExporting;
    }

    // "Reporte de compras 2026-10-01 a 2026-10-23"
    private string SuggestedFileName()
    {
        DateTime first = _startLocal!.Value;
        DateTime last = _endLocal!.Value.AddDays(-1);

        return first.Date == last.Date
            ? $"Reporte de compras {first:yyyy-MM-dd}"
            : $"Reporte de compras {first:yyyy-MM-dd} a {last:yyyy-MM-dd}";
    }

    private string CompanyName()
    {
        try
        {
            string name = _settings.GetTicketHeader().CompanyName;

            return string.IsNullOrWhiteSpace(name) ? "EcoPapel" : name.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible leer el nombre de la empresa para el reporte.");
            return "EcoPapel";
        }
    }

    private async Task ExportAsync(string extension)
    {
        if (_report is not { IsEmpty: false } report)
            return;

        string? path = _dialogs.ShowSaveDialog(SuggestedFileName(), extension);

        if (path is null)
            return;

        var info = new ReportExportInfo(
            CompanyName(),
            RangeText,
            TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), _clock.LocalTimeZone).DateTime,
            _clock.LocalTimeZone);

        IsExporting = true;

        try
        {
            // Fuera del hilo de la interfaz: un periodo largo puede tardar
            await Task.Run(() =>
            {
                using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

                if (extension == "pdf")
                    _exporter.WritePdf(report, info, stream);
                else
                    _exporter.WriteExcel(report, info, stream);
            });
        }
        catch (IOException ex)
        {
            _notifications.ShowError("No se pudo guardar el archivo. Si está abierto, ciérralo e intenta de nuevo.");

            _logger.LogWarning(ex, "No fue posible guardar el reporte en {Path}.", path);
            return;
        }
        catch (Exception ex)
        {
            _notifications.ShowError("No fue posible exportar el reporte.");

            _logger.LogError(ex, "No fue posible exportar el reporte a {Path}.", path);
            return;
        }
        finally
        {
            IsExporting = false;
        }

        _notifications.ShowSuccess($"Reporte guardado: {Path.GetFileName(path)}");

        try
        {
            _dialogs.OpenFile(path);
        }
        catch (Exception ex)
        {
            // Ya se guardo: si no hay programa para abrirlo, basta con el aviso
            _logger.LogWarning(ex, "No fue posible abrir el reporte {Path}.", path);
        }
    }
}
