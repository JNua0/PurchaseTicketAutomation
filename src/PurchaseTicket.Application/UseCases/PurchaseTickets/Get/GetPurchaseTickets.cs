using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Get;

public class GetPurchaseTickets
{
    private readonly IPurchaseTicketRepository _repository;

    public GetPurchaseTickets(
        IPurchaseTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PurchaseTicketDto>> ExecuteAsync()
    {
        var tickets = await _repository.GetAllAsync();

        return tickets
            .Select(ticket => new PurchaseTicketDto(
                ticket.Id,
                ticket.TicketNumber,
                ticket.CreatedAt,
                ticket.SupplierId,
                ticket.MaterialId,
                ticket.LicensePlate,
                ticket.DriverName,
                ticket.GrossWeight,
                ticket.TareWeight,
                ticket.NetWeight,
                ticket.Discount,
                ticket.DiscountWeight,
                ticket.NetWeightAfterDiscount,
                ticket.PricePerKg,
                ticket.Amount,
                ticket.Status.ToString()))
            .ToList();
    }
}