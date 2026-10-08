using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;

namespace PurchaseTicket.Application.UseCases.Suppliers.Get;

public class GetSuppliers
{
    private readonly ISupplierQueryRepository _repository;

    public GetSuppliers(ISupplierQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<SupplierDto>> ExecuteAsync(GetSuppliersQuery query)
    {
        if (query.Page < 1)
            throw new ArgumentOutOfRangeException(nameof(query.Page));

        if (query.PageSize < 1 || query.PageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(query.PageSize));

        var search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim();

        var result = await _repository.GetPagedAsync(
            query.Page,
            query.PageSize,
            search,
            query.IsActive);

        var items = result.Items
            .Select(supplier => new SupplierDto(
                supplier.Id,
                supplier.Name,
                supplier.PhoneNumber,
                supplier.IsActive))
            .ToList();

        return new PagedResult<SupplierDto>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}