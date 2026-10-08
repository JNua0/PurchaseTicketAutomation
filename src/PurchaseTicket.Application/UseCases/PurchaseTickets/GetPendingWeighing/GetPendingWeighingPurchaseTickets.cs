using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetPendingWeighing;

public class GetPendingWeighingPurchaseTickets
{
    private readonly IPurchaseTicketRepository _repository;

    public GetPendingWeighingPurchaseTickets(
        IPurchaseTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PendingWeighingPurchaseTicketDto>> ExecuteAsync()
    {
        var tickets =
            await _repository.GetPendingWeighingAsync();

        return tickets
            .Select(ticket => new PendingWeighingPurchaseTicketDto(
                ticket.TicketNumber,
                ticket.CheckInAt,
                ticket.LicensePlate
                    ?? throw new InvalidOperationException(
                        "A pending weighing ticket must have a license plate."),
                ticket.Transporter,
                ticket.GrossWeight
                    ?? throw new InvalidOperationException(
                        "A pending weighing ticket must have a gross weight.")))
            .ToList();
    }
}