namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Complete;

public record CompletePurchaseTicketResult(
    string TicketNumber,
    bool Printed);