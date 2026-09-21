namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Complete;

public record CompletePurchaseTicketCommand(
    int TicketId,
    decimal TareWeight,
    decimal Discount,
    decimal PricePerKg);