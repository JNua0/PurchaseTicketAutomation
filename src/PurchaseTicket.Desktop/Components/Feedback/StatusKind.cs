namespace PurchaseTicket.Desktop.Components.Feedback;

// Tipo de estado: define los colores de StatusBadge.
// Info marca un paso intermedio sin alerta, como "Pendiente de importe"
public enum StatusKind
{
    Neutral,
    Info,
    Success,
    Warning,
    Error
}