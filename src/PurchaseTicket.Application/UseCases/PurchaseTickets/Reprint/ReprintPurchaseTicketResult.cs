namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Reprint;

public record ReprintPurchaseTicketResult(
    string TicketNumber,
    bool Printed);