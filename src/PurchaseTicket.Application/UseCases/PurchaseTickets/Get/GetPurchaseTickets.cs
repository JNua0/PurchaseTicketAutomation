using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Common;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Get;

public class GetPurchaseTickets
{
    private readonly IPurchaseTicketQueryRepository _repository;

    public GetPurchaseTickets(
        IPurchaseTicketQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<PurchaseTicketDto>> ExecuteAsync(
        GetPurchaseTicketsQuery query)
    {
        if (query.Page < 1)
            throw new ArgumentException(
                "Page must be greater than zero.");

        if (query.PageSize < 1 ||
            query.PageSize > 100)
            throw new ArgumentException(
                "Page size must be between 1 and 100.");

        var result = await _repository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.Status,
            query.WeighingType);

        var items = result.Items
            .Select(item => new PurchaseTicketDto(
                item.TicketNumber,
                item.CheckInAt,
                item.SupplierName,
                item.MaterialName,
                item.LicensePlate,
                item.Transporter,
                item.FinalWeight,
                item.Amount,
                item.WeighingType,
                item.Status))
            .ToList();

        return new PagedResult<PurchaseTicketDto>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}