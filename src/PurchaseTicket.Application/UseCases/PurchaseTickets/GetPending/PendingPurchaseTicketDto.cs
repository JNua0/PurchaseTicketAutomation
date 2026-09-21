namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetPending;

public record PendingPurchaseTicketDto(
    int Id,
    string TicketNumber,
    DateTime CreatedAt,
    string LicensePlate,
    string DriverName,
    decimal GrossWeight);