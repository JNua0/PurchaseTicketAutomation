using System.Globalization;
using PurchaseTicket.Application.UseCases.Reports.Purchases;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.Reports;

// Un renglon de Por material o Por proveedor, con el formato de la pantalla
public sealed class ReportGroupRow
{
    // Campos

    // Ancho de la barra del porcentaje cuando es el 100 %
    public const double BarWidth = 120;

    // Constructor

    public ReportGroupRow(PurchaseReportGroup group, decimal share)
    {
        Name = group.IsActive ? group.Name : $"{group.Name} (inactivo)";
        IsInactive = !group.IsActive;
        CountText = group.TicketCount.ToString("N0", CultureInfo.CurrentCulture);
        NetWeightText = DisplayFormat.WholeKg(group.NetWeight);
        ShareText = $"{Math.Round(share * 100m, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.CurrentCulture)} %";
        ShareBarWidth = (double)share * BarWidth;

        // Todos pendientes: todavia no hay importe que sumar
        AmountText = group.PendingAmountCount == group.TicketCount ? "—" : DisplayFormat.Money(group.Amount);
        PendingText = group.PendingAmountCount == 0 ? string.Empty : $"{group.PendingAmountCount} sin importe";
    }

    // Propiedades

    public string Name { get; }

    public bool IsInactive { get; }

    public string CountText { get; }

    public string NetWeightText { get; }

    public string ShareText { get; }

    public double ShareBarWidth { get; }

    public string AmountText { get; }

    public string PendingText { get; }

    public bool HasPending => PendingText.Length > 0;
}
