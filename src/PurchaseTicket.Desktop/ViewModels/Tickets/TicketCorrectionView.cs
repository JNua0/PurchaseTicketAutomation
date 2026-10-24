using PurchaseTicket.Application.Abstractions.Persistence.Models;

namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Una correccion del historial lista para mostrar en el detalle del ticket
public sealed class TicketCorrectionView
{
    // Constructor

    public TicketCorrectionView(TicketCorrectionItem item, string dateText)
    {
        DateText = dateText;
        Reason = item.Reason ?? string.Empty;
        Changes = item.Changes.Select(change => new TicketChangeView(change)).ToList();
        CountText = Changes.Count == 1 ? "1 cambio" : $"{Changes.Count} cambios";
    }

    // Propiedades

    public string DateText { get; }

    public string CountText { get; }

    public string Reason { get; }

    public bool HasReason => Reason.Length > 0;

    public IReadOnlyList<TicketChangeView> Changes { get; }
}
