namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Create;

public record CreatePurchaseTicketCommand(
    int SupplierCustomerId,
    int MaterialId,
    string LicensePlate,
    string DriverName,
    decimal GrossWeight);