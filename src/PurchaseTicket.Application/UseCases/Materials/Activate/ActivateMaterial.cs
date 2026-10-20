using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.Materials.Activate;

public class ActivateMaterial
{
    private readonly IMaterialRepository _repository;

    public ActivateMaterial(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(int materialId)
    {
        var material = await _repository.GetByIdAsync(materialId);

        if (material is null)
        {
            throw new InvalidOperationException(
                "No se encontró el material.");
        }

        material.Activate();

        await _repository.UpdateAsync(material);
    }
}