using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Infrastructure.Persistence.Repositories;

public class PurchaseTicketRepository : IPurchaseTicketRepository
{
    private readonly ApplicationDbContext _context;

    public PurchaseTicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Ticket purchaseTicket)
    {
        await _context.PurchaseTickets.AddAsync(purchaseTicket);
        await _context.SaveChangesAsync();
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        return await _context.PurchaseTickets
            .FirstOrDefaultAsync(ticket => ticket.Id == id);
    }

    public async Task UpdateAsync(Ticket purchaseTicket)
    {
        _context.PurchaseTickets.Update(purchaseTicket);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Ticket>> GetPendingWeighingAsync()
    {
        return await _context.PurchaseTickets
            .AsNoTracking()
            .Where(ticket => ticket.Status == TicketStatus.WeighingPending)
            .ToListAsync();
    }
}