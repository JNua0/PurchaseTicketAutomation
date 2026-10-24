using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;

namespace PurchaseTicket.Infrastructure.Persistence.Repositories;

public class TicketCorrectionQueryRepository
    : ITicketCorrectionQueryRepository
{
    private readonly ApplicationDbContext _context;

    public TicketCorrectionQueryRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TicketCorrectionItem>> GetByTicketIdAsync(int ticketId)
    {
        // Los cambios se leen con la fila: estan en su columna jsonb
        var corrections = await _context.TicketCorrections
            .AsNoTracking()
            .Where(correction => correction.PurchaseTicketId == ticketId)
            .OrderByDescending(correction => correction.CorrectedAt)
            .ThenByDescending(correction => correction.Id)
            .ToListAsync();

        return corrections
            .Select(correction => new TicketCorrectionItem(
                correction.Id,
                correction.CorrectedAt,
                correction.Reason,
                correction.Changes.ToList()))
            .ToList();
    }
}
