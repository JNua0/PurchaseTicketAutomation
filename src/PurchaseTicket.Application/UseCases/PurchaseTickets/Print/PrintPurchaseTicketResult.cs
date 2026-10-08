namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Print;

public record PrintPurchaseTicketResult(
    string TicketNumber,
    bool Printed);