using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Abstractions.Persistence;

public interface IPurchaseTicketRepository
{
    Task AddAsync(Ticket purchaseTicket);
    Task<Ticket?> GetByIdAsync(int id);
    Task UpdateAsync(Ticket purchaseTicket);
    Task<IReadOnlyList<Ticket>> GetPendingAsync();
}