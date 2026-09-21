using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Abstractions.Persistence;

public interface IPurchaseTicketRepository
{
    Task AddAsync(Ticket purchaseTicket);
}