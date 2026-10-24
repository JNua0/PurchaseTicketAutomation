namespace PurchaseTicket.Application.Abstractions.Reports;

// Datos del encabezado del archivo exportado. Las fechas de los tickets se
// muestran en la zona horaria indicada
public sealed record ReportExportInfo(
    string CompanyName,
    string PeriodText,
    DateTime GeneratedAt,
    TimeZoneInfo TimeZone);
