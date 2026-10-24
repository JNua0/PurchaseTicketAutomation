using PurchaseTicket.Application.UseCases.Reports.Purchases;

namespace PurchaseTicket.Application.Abstractions.Reports;

// Escribe el reporte en un archivo. Excel lleva Resumen, Por material,
// Por proveedor y Tickets; el PDF, el resumen y los dos desgloses
public interface IPurchaseReportExporter
{
    void WriteExcel(PurchaseReport report, ReportExportInfo info, Stream output);

    void WritePdf(PurchaseReport report, ReportExportInfo info, Stream output);
}
