using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Resultado de Nuevo pesaje para la lista. PrintWarning trae el motivo
// si se pidio imprimir y no se pudo; null si se imprimio o no se pidio
public sealed record WeighingRegistration(
    string TicketNumber,
    WeighingType WeighingType,
    string? PrintWarning);