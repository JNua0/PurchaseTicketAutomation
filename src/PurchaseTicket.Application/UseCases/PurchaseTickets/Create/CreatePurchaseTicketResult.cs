namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Create;

public record CreatePurchaseTicketResult(
    string TicketNumber,
    bool Printed);