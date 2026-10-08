namespace PurchaseTicket.Application.UseCases.PurchaseTickets.CreateConventional;

public record CreateConventionalPurchaseTicketCommand(
    int SupplierId,
    int MaterialId,
    string LicensePlate,
    string Transporter,
    decimal GrossWeight);