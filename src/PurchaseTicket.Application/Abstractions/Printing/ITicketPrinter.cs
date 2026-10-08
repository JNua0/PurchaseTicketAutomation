using PurchaseTicket.Application.Abstractions.Printing;

public interface ITicketPrinter
{
    Task PrintConventionalInitialAsync(TicketPrintData data);
    Task PrintConventionalFinalAsync(TicketPrintData data);
    Task PrintConventionalCompletedAsync(TicketPrintData data);
    Task PrintSingleAsync(TicketPrintData data);
}