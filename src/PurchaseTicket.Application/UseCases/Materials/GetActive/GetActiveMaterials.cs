using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.Materials.GetActive;

public class GetActiveMaterials
{
    private readonly IMaterialRepository _repository;

    public GetActiveMaterials(
        IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MaterialOptionDto>> ExecuteAsync()
    {
        var materials =
            await _repository.GetActiveAsync();

        return materials
            .Select(material => new MaterialOptionDto(
                material.Id,
                material.Name))
            .ToList();
    }
}