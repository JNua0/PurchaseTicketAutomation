using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Abstractions.Printing;

public interface ITicketPrinter
{
    Task PrintInitialAsync(Ticket purchaseTicket);
    Task PrintFinalAsync(Ticket purchaseTicket);
}