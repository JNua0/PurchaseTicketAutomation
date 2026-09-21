using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetPending;

public class GetPendingPurchaseTickets
{
    private readonly IPurchaseTicketRepository _repository;

    public GetPendingPurchaseTickets(
        IPurchaseTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PendingPurchaseTicketDto>> ExecuteAsync()
    {
        var tickets = await _repository.GetPendingAsync();

        return tickets
            .Select(ticket => new PendingPurchaseTicketDto(
                ticket.Id,
                ticket.TicketNumber,
                ticket.CreatedAt,
                ticket.LicensePlate,
                ticket.DriverName,
                ticket.GrossWeight))
            .ToList();
    }
}