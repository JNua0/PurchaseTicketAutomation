namespace PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterAmount;

public record RegisterAmountCommand(
    int TicketId,
    int Discount,
    decimal PricePerKg);