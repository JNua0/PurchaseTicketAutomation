namespace PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterAmount;

public record RegisterAmountCommand(
    int TicketId,
    decimal Discount,
    decimal PricePerKg);