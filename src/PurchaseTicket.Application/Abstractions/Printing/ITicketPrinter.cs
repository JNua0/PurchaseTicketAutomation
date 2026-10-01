namespace PurchaseTicket.Application.Abstractions.Printing;

public interface ITicketPrinter
{
    Task PrintInitialAsync(TicketPrintData data);
    Task PrintFinalAsync(TicketPrintData data);
}