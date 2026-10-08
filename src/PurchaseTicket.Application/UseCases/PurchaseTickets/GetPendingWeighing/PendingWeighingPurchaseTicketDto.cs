namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetPendingWeighing;

public record PendingWeighingPurchaseTicketDto(
    string TicketNumber,
    DateTime CheckInAt,
    string LicensePlate,
    string Transporter,
    decimal GrossWeight);