namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Create;

public record CreatePurchaseTicketCommand(
    int SupplierId,
    int MaterialId,
    string LicensePlate,
    string DriverName,
    decimal GrossWeight);