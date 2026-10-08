namespace PurchaseTicket.Application.UseCases.PurchaseTickets.CreateSingle;

public record CreateSinglePurchaseTicketCommand(
    int SupplierId,
    int MaterialId,
    string? LicensePlate,
    string Transporter,
    decimal NetWeight);