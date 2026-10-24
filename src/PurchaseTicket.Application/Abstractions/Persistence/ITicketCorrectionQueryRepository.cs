using PurchaseTicket.Application.Abstractions.Persistence.Models;

namespace PurchaseTicket.Application.Abstractions.Persistence;

public interface ITicketCorrectionQueryRepository
{
    // Correcciones de un ticket, la mas reciente primero; vacia si no tiene
    Task<IReadOnlyList<TicketCorrectionItem>> GetByTicketIdAsync(int ticketId);
}
