using ClosedXML.Excel;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Abstractions.Reports;
using PurchaseTicket.Application.UseCases.Reports.Purchases;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Infrastructure.Reports;

namespace PurchaseTicket.Infrastructure.Tests.Reports;

public class PurchaseReportExporterTests
{
    private static readonly DateTime From = new(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc);

    private static PurchaseReport CreateReport()
    {
        ReportTicket[] tickets =
        [
            new("10", From.AddHours(3), 1, "Proveedor Uno", true, 1, "Carton", true,
                "ABC123", "Juan Perez", WeighingType.Conventional, TicketStatus.Completed,
                20000m, 10, 18000m, 2m, 36000m),
            new("11", From.AddHours(5), 2, "Proveedor Dos", false, 2, "Archivo", true,
                null, "Ana Lopez", WeighingType.Single, TicketStatus.AmountPending,
                12000m, null, null, null, null)
        ];

        return PurchaseReport.Create(From, From.AddDays(23), tickets);
    }

    private static ReportExportInfo CreateInfo()
    {
        return new ReportExportInfo(
            "EcoPapel Recycling",
            "1 al 23 de octubre de 2026",
            new DateTime(2026, 10, 23, 21, 0, 0),
            TimeZoneInfo.Utc);
    }

    [Fact]
    public void WriteExcel_ShouldWriteFourSheetsWithTickets()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        new PurchaseReportExporter().WriteExcel(CreateReport(), CreateInfo(), stream);

        // Assert
        stream.Position = 0;
        using var workbook = new XLWorkbook(stream);

        Assert.Equal(
            new[] { "Resumen", "Por material", "Por proveedor", "Tickets" },
            workbook.Worksheets.Select(sheet => sheet.Name).ToArray());

        var ticketsSheet = workbook.Worksheet("Tickets");
        Assert.Equal("10", ticketsSheet.Cell(2, 1).GetString());
        Assert.Equal(20000d, ticketsSheet.Cell(2, 9).GetDouble());
        Assert.True(ticketsSheet.Cell(3, 13).IsEmpty());

        var suppliersSheet = workbook.Worksheet("Por proveedor");
        Assert.Equal("Proveedor Uno", suppliersSheet.Cell(2, 1).GetString());
        Assert.Equal("Proveedor Dos (inactivo)", suppliersSheet.Cell(3, 1).GetString());
        Assert.Equal("Total", suppliersSheet.Cell(4, 1).GetString());
    }

    [Fact]
    public void WritePdf_ShouldWriteAPdfDocument()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        new PurchaseReportExporter().WritePdf(CreateReport(), CreateInfo(), stream);

        // Assert: todo PDF empieza con "%PDF"
        byte[] bytes = stream.ToArray();
        Assert.True(bytes.Length > 1000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }
}
