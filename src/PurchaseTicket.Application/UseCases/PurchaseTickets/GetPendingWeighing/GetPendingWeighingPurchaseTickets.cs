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
                ticket.Id,
                ticket.TicketNumber,
                ticket.CheckInAt,
                ticket.LicensePlate
                    ?? throw new InvalidOperationException(
                        "Un ticket pendiente de pesaje debe tener placas."),
                ticket.Transporter,
                ticket.GrossWeight
                    ?? throw new InvalidOperationException(
                        "Un ticket pendiente de pesaje debe tener peso bruto.")))
            .ToList();
    }
}