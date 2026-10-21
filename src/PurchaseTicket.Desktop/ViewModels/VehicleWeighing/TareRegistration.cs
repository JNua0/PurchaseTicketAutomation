namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Resultado de Registrar tara para la lista. PrintWarning trae el motivo
// si se pidio imprimir y no se pudo; null si se imprimio o no se pidio
public sealed record TareRegistration(
    string TicketNumber,
    decimal NetWeight,
    string? PrintWarning);