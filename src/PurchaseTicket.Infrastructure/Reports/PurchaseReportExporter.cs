using System.Globalization;
using ClosedXML.Excel;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Abstractions.Reports;
using PurchaseTicket.Application.UseCases.Reports.Purchases;
using PurchaseTicket.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PurchaseTicket.Infrastructure.Reports;

// Excel con ClosedXML y PDF con QuestPDF. Los numeros van como numeros, no
// como texto, para que en Excel se puedan sumar y filtrar
public sealed class PurchaseReportExporter : IPurchaseReportExporter
{
    // Campos

    private const string KgFormat = "#,##0 \"kg\"";
    private const string DecimalKgFormat = "#,##0.00 \"kg\"";
    private const string MoneyFormat = "$#,##0.00";
    private const string PercentFormat = "0.0%";
    private const string DateTimeFormat = "dd/mm/yyyy hh:mm";

    private const string SecondaryColor = "#5F6B76";
    private const string BorderColor = "#D9E5EA";
    private const string AccentColor = "#0A63C9";

    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-MX");

    // Constructor

    static PurchaseReportExporter()
    {
        // Licencia Community de QuestPDF: gratuita para empresas con ingresos
        // anuales menores a 1 millon de dolares
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // Metodos publicos

    public void WriteExcel(PurchaseReport report, ReportExportInfo info, Stream output)
    {
        using var workbook = new XLWorkbook();

        WriteSummarySheet(workbook.Worksheets.Add("Resumen"), report, info);
        WriteGroupSheet(workbook.Worksheets.Add("Por material"), "Material", report.ByMaterial, report);
        WriteGroupSheet(workbook.Worksheets.Add("Por proveedor"), "Proveedor", report.BySupplier, report);
        WriteTicketsSheet(workbook.Worksheets.Add("Tickets"), report, info);

        workbook.SaveAs(output);
    }

    public void WritePdf(PurchaseReport report, ReportExportInfo info, Stream output)
    {
        Document
            .Create(container => container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(36);
                page.DefaultTextStyle(style => style.FontSize(9).FontFamily("Segoe UI"));

                page.Header().Column(column =>
                {
                    column.Item().Text(info.CompanyName).FontSize(14).Bold();
                    column.Item().Text("Reporte de compras").FontSize(12).SemiBold();
                    column.Item().Text($"{info.PeriodText} · según la fecha del último pesaje").FontColor(SecondaryColor);
                    column.Item().PaddingBottom(10).Text($"Generado el {info.GeneratedAt.ToString("dd/MM/yyyy HH:mm", Spanish)}").FontColor(SecondaryColor);
                });

                page.Content().Column(column =>
                {
                    column.Spacing(16);

                    column.Item().Row(row =>
                    {
                        row.Spacing(8);

                        AddTile(row, "Tickets", report.TicketCount.ToString("N0", Spanish),
                            $"{report.CompletedCount} completados · {report.PendingAmountCount} pendientes de importe");
                        AddTile(row, "Peso neto", Kg(report.NetWeight), "Lo que marcó la báscula");
                        AddTile(row, "Peso neto total", DecimalKg(report.NetWeightAfterDiscount), "Con descuento; solo completados");
                        AddTile(row, "Importe", Money(report.Amount), "Solo completados");
                    });

                    if (report.IsEmpty)
                    {
                        column.Item().Text("No hay tickets completados ni pendientes de importe en este periodo.").FontColor(SecondaryColor);
                        return;
                    }

                    column.Item().Element(item => GroupTable(item, "Por material", "Material", report.ByMaterial, report));
                    column.Item().Element(item => GroupTable(item, "Por proveedor", "Proveedor", report.BySupplier, report));
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontColor(SecondaryColor));
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            }))
            .GeneratePdf(output);
    }

    // Metodos privados

    private static string Kg(decimal value) => $"{value.ToString("N0", Spanish)} kg";

    private static string DecimalKg(decimal value) => $"{value.ToString("N2", Spanish)} kg";

    private static string Money(decimal value) => $"${value.ToString("N2", Spanish)}";

    private static string Percent(decimal share) => $"{(share * 100m).ToString("0.0", Spanish)} %";

    private static string StatusText(TicketStatus status) => status switch
    {
        TicketStatus.Completed => "Completado",
        TicketStatus.AmountPending => "Pendiente de importe",
        TicketStatus.WeighingPending => "Pendiente de pesaje",
        TicketStatus.Cancelled => "Cancelado",
        _ => status.ToString()
    };

    private static string WeighingTypeText(WeighingType type) =>
        type == WeighingType.Conventional ? "Convencional" : "Un solo pesaje";

    private static string GroupName(PurchaseReportGroup group) =>
        group.IsActive ? group.Name : $"{group.Name} (inactivo)";

    // ---- Excel ----

    private static void SetNumber(IXLCell cell, decimal? value, string format)
    {
        if (value is not decimal number)
            return;

        cell.Value = (double)number;
        cell.Style.NumberFormat.Format = format;
    }

    private static void StyleHeader(IXLRange header)
    {
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF3FF");
        header.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
    }

    private static void WriteSummarySheet(IXLWorksheet sheet, PurchaseReport report, ReportExportInfo info)
    {
        sheet.Cell(1, 1).Value = info.CompanyName;
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;

        sheet.Cell(2, 1).Value = "Reporte de compras";
        sheet.Cell(2, 1).Style.Font.Bold = true;

        sheet.Cell(3, 1).Value = $"{info.PeriodText} · según la fecha del último pesaje";
        sheet.Cell(4, 1).Value = $"Generado el {info.GeneratedAt.ToString("dd/MM/yyyy HH:mm", Spanish)}";

        int row = 6;

        void AddTotal(string label, decimal value, string format, string note)
        {
            sheet.Cell(row, 1).Value = label;
            SetNumber(sheet.Cell(row, 2), value, format);
            sheet.Cell(row, 3).Value = note;
            sheet.Cell(row, 3).Style.Font.FontColor = XLColor.FromHtml(SecondaryColor);
            row++;
        }

        AddTotal("Tickets", report.TicketCount, "#,##0", "Completados y pendientes de importe");
        AddTotal("Completados", report.CompletedCount, "#,##0", string.Empty);
        AddTotal("Pendientes de importe", report.PendingAmountCount, "#,##0", "Cuentan en kilos, todavía sin importe");
        AddTotal("Peso neto", report.NetWeight, KgFormat, "Lo que marcó la báscula");
        AddTotal("Peso neto total", report.NetWeightAfterDiscount, DecimalKgFormat, "Con descuento; solo completados");
        AddTotal("Importe", report.Amount, MoneyFormat, "Solo completados");

        sheet.Range(6, 1, row - 1, 1).Style.Font.Bold = true;
        sheet.Columns(1, 3).AdjustToContents();
    }

    private static void WriteGroupSheet(
        IXLWorksheet sheet,
        string nameHeader,
        IReadOnlyList<PurchaseReportGroup> groups,
        PurchaseReport report)
    {
        string[] headers = [nameHeader, "Tickets", "Sin importe", "Peso neto", "% del peso", "Importe"];

        for (int column = 0; column < headers.Length; column++)
            sheet.Cell(1, column + 1).Value = headers[column];

        StyleHeader(sheet.Range(1, 1, 1, headers.Length));

        int row = 2;

        foreach (var group in groups)
        {
            sheet.Cell(row, 1).Value = GroupName(group);
            SetNumber(sheet.Cell(row, 2), group.TicketCount, "#,##0");
            SetNumber(sheet.Cell(row, 3), group.PendingAmountCount, "#,##0");
            SetNumber(sheet.Cell(row, 4), group.NetWeight, KgFormat);
            SetNumber(sheet.Cell(row, 5), report.ShareOf(group), PercentFormat);
            SetNumber(sheet.Cell(row, 6), group.Amount, MoneyFormat);
            row++;
        }

        sheet.Cell(row, 1).Value = "Total";
        SetNumber(sheet.Cell(row, 2), report.TicketCount, "#,##0");
        SetNumber(sheet.Cell(row, 3), report.PendingAmountCount, "#,##0");
        SetNumber(sheet.Cell(row, 4), report.NetWeight, KgFormat);
        SetNumber(sheet.Cell(row, 5), report.IsEmpty ? 0m : 1m, PercentFormat);
        SetNumber(sheet.Cell(row, 6), report.Amount, MoneyFormat);
        sheet.Range(row, 1, row, headers.Length).Style.Font.Bold = true;
        sheet.Range(row, 1, row, headers.Length).Style.Border.TopBorder = XLBorderStyleValues.Thin;

        sheet.SheetView.FreezeRows(1);
        sheet.Columns(1, headers.Length).AdjustToContents();
    }

    private static void WriteTicketsSheet(IXLWorksheet sheet, PurchaseReport report, ReportExportInfo info)
    {
        string[] headers =
        [
            "Folio", "Último pesaje", "Proveedor", "Material", "Placas", "Chofer", "Modalidad",
            "Estado", "Peso neto", "Descuento (%)", "Peso neto total", "Precio por kg", "Importe"
        ];

        for (int column = 0; column < headers.Length; column++)
            sheet.Cell(1, column + 1).Value = headers[column];

        StyleHeader(sheet.Range(1, 1, 1, headers.Length));

        int row = 2;

        foreach (ReportTicket ticket in report.Tickets)
        {
            DateTime local = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(ticket.LastWeighingAt, DateTimeKind.Utc),
                info.TimeZone);

            sheet.Cell(row, 1).Value = ticket.TicketNumber;
            sheet.Cell(row, 2).Value = local;
            sheet.Cell(row, 2).Style.NumberFormat.Format = DateTimeFormat;
            sheet.Cell(row, 3).Value = ticket.SupplierName;
            sheet.Cell(row, 4).Value = ticket.MaterialName;
            sheet.Cell(row, 5).Value = ticket.LicensePlate ?? string.Empty;
            sheet.Cell(row, 6).Value = ticket.Transporter;
            sheet.Cell(row, 7).Value = WeighingTypeText(ticket.WeighingType);
            sheet.Cell(row, 8).Value = StatusText(ticket.Status);
            SetNumber(sheet.Cell(row, 9), ticket.NetWeight, KgFormat);
            SetNumber(sheet.Cell(row, 10), ticket.Discount, "0");
            SetNumber(sheet.Cell(row, 11), ticket.NetWeightAfterDiscount, DecimalKgFormat);
            SetNumber(sheet.Cell(row, 12), ticket.PricePerKg, MoneyFormat);
            SetNumber(sheet.Cell(row, 13), ticket.Amount, MoneyFormat);
            row++;
        }

        // Filtros en el encabezado para buscar por proveedor, material o estado
        sheet.Range(1, 1, Math.Max(1, row - 1), headers.Length).SetAutoFilter();
        sheet.SheetView.FreezeRows(1);
        sheet.Columns(1, headers.Length).AdjustToContents();
    }

    // ---- PDF ----

    private static void AddTile(RowDescriptor row, string label, string value, string note)
    {
        row.RelativeItem()
            .Border(1)
            .BorderColor(BorderColor)
            .Padding(8)
            .Column(column =>
            {
                column.Item().Text(label).FontColor(SecondaryColor);
                column.Item().Text(value).FontSize(13).Bold();
                column.Item().Text(note).FontSize(7).FontColor(SecondaryColor);
            });
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(BorderColor)
            .PaddingVertical(4)
            .DefaultTextStyle(style => style.SemiBold().FontColor(SecondaryColor));
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .BorderBottom(0.5f)
            .BorderColor("#EEF3F6")
            .PaddingVertical(3);
    }

    private static IContainer TotalCell(IContainer container)
    {
        return container
            .PaddingVertical(4)
            .DefaultTextStyle(style => style.Bold());
    }

    private static void GroupTable(
        IContainer container,
        string title,
        string nameHeader,
        IReadOnlyList<PurchaseReportGroup> groups,
        PurchaseReport report)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(4).Text($"{title} · {groups.Count}").FontSize(11).SemiBold();

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(5);
                    columns.ConstantColumn(48);
                    columns.ConstantColumn(56);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(48);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text(nameHeader);
                    header.Cell().Element(HeaderCell).AlignRight().Text("Tickets");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Sin importe");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Peso neto");
                    header.Cell().Element(HeaderCell).AlignRight().Text("%");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Importe");
                });

                foreach (var group in groups)
                {
                    table.Cell().Element(BodyCell).Text(GroupName(group));
                    table.Cell().Element(BodyCell).AlignRight().Text(group.TicketCount.ToString("N0", Spanish));
                    table.Cell().Element(BodyCell).AlignRight().Text(group.PendingAmountCount == 0 ? "—" : group.PendingAmountCount.ToString("N0", Spanish));
                    table.Cell().Element(BodyCell).AlignRight().Text(Kg(group.NetWeight));
                    table.Cell().Element(BodyCell).AlignRight().Text(Percent(report.ShareOf(group)));
                    table.Cell().Element(BodyCell).AlignRight().Text(group.TicketCount == group.PendingAmountCount ? "—" : Money(group.Amount)).FontColor(AccentColor);
                }

                table.Cell().Element(TotalCell).Text("Total");
                table.Cell().Element(TotalCell).AlignRight().Text(report.TicketCount.ToString("N0", Spanish));
                table.Cell().Element(TotalCell).AlignRight().Text(report.PendingAmountCount.ToString("N0", Spanish));
                table.Cell().Element(TotalCell).AlignRight().Text(Kg(report.NetWeight));
                table.Cell().Element(TotalCell).AlignRight().Text("100 %");
                table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Amount));
            });
        });
    }
}
