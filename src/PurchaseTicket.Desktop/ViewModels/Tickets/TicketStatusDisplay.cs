using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Texto y color de cada estado; lo usan la lista y el detalle de Tickets
public static class TicketStatusDisplay
{
    // Metodos publicos

    public static string Text(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.WeighingPending => "Pendiente de pesaje",
            TicketStatus.AmountPending => "Pendiente de importe",
            TicketStatus.Completed => "Completado",
            _ => "Cancelado"
        };
    }

    // Ambar espera la tara, azul espera el importe, verde termino, rojo cancelado
    public static StatusKind Kind(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.WeighingPending => StatusKind.Warning,
            TicketStatus.AmountPending => StatusKind.Info,
            TicketStatus.Completed => StatusKind.Success,
            _ => StatusKind.Error
        };
    }

    public static string WeighingTypeText(WeighingType weighingType)
    {
        return weighingType == WeighingType.Conventional
            ? "Convencional"
            : "Un solo pesaje";
    }
}