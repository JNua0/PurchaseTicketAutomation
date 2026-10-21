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
                "La página debe ser mayor a cero.");

        if (query.PageSize < 1 ||
            query.PageSize > 100)
            throw new ArgumentException(
                "El tamaño de página debe estar entre 1 y 100.");

        if (query.WeighedFrom.HasValue &&
            query.WeighedTo.HasValue &&
            query.WeighedFrom.Value >= query.WeighedTo.Value)
            throw new ArgumentException(
                "La fecha inicial debe ser anterior a la fecha final.");

        var result = await _repository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.Status,
            query.WeighingType,
            query.WeighedFrom,
            query.WeighedTo);

        var items = result.Items
            .Select(item => new PurchaseTicketDto(
                item.Id,
                item.TicketNumber,
                item.CheckInAt,
                item.DepartureAt,
                item.SupplierId,
                item.MaterialId,
                item.SupplierName,
                item.MaterialName,
                item.LicensePlate,
                item.Transporter,
                item.GrossWeight,
                item.NetWeight,
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