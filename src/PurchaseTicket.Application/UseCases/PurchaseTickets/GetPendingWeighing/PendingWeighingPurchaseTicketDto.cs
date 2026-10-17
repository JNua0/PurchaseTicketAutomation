namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetPendingWeighing;

public record PendingWeighingPurchaseTicketDto(
    int Id,
    string TicketNumber,
    DateTime CheckInAt,
    string LicensePlate,
    string Transporter,
    decimal GrossWeight);
