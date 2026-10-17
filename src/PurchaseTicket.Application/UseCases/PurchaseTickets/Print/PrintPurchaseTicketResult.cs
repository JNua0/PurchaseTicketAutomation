using PurchaseTicket.Application.Abstractions.Printing;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Print;

public record PrintPurchaseTicketResult(
    string TicketNumber,
    bool Printed,
    PrinterErrorCode ErrorCode = PrinterErrorCode.None);