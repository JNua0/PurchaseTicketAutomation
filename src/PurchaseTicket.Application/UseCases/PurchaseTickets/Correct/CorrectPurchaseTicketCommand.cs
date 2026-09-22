namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;

public record CorrectPurchaseTicketCommand(
    int TicketId,
    int SupplierId,
    int MaterialId,
    string LicensePlate,
    string DriverName,
    decimal GrossWeight);