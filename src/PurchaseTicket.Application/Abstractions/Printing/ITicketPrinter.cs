namespace PurchaseTicket.Application.Abstractions.Printing;

public interface ITicketPrinter
{
    Task PrintConventionalInitialAsync(TicketPrintData data);

    Task PrintConventionalFinalAsync(TicketPrintData data);

    Task PrintConventionalCompletedAsync(TicketPrintData data);

    Task PrintSingleAsync(TicketPrintData data);

    // Ticket sin datos de compra para comprobar el puerto desde Configuracion
    Task PrintTestAsync();
}