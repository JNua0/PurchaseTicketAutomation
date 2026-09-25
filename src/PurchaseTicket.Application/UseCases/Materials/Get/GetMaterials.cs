using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.UseCases.Materials.Get;

public class GetMaterials
{
    private readonly IMaterialRepository _repository;

    public GetMaterials(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Material>> ExecuteAsync(
        string? searchTerm = null)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _repository.GetAllAsync();
        }

        return await _repository.SearchByNameAsync(
            searchTerm.Trim());
    }
}