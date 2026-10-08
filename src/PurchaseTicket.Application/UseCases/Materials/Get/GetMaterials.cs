using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Common;

namespace PurchaseTicket.Application.UseCases.Materials.Get;

public class GetMaterials
{
    private readonly IMaterialQueryRepository _repository;

    public GetMaterials(
        IMaterialQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<MaterialDto>> ExecuteAsync(GetMaterialsQuery query)
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
            .Select(material => new MaterialDto(
                material.Id,
                material.Name,
                material.IsActive))
            .ToList();

        return new PagedResult<MaterialDto>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}