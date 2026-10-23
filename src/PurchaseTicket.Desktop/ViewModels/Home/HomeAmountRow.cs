using PurchaseTicket.Desktop.ViewModels.Amounts;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.Home;

// Fila de "Pendientes de importe": el ticket y si se peso antes de hoy
public sealed record HomeAmountRow(
    AmountListItem Ticket,
    bool IsFromPreviousDay)
{
    // Propiedades

    public string NetWeightText =>
        Ticket.NetWeight is decimal weight ? DisplayFormat.WholeKg(weight) : string.Empty;

    public string WeighedDate => Ticket.WeighedAt.ToString("dd/MM/yyyy");
}
