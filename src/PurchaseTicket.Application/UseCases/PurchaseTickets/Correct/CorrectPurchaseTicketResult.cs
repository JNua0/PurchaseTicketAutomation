namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;

public record CorrectPurchaseTicketResult(
    string TicketNumber,
    bool Printed);