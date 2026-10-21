using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Common;

namespace PurchaseTicket.Application.UseCases.Materials.Deactivate;

public class DeactivateMaterial
{
    private readonly IMaterialRepository _repository;

    public DeactivateMaterial(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(int materialId)
    {
        var material = await _repository.GetByIdAsync(materialId);

        if (material is null)
        {
            throw new NotFoundException(
                "No se encontró el material.");
        }

        material.Deactivate();

        await _repository.UpdateAsync(material);
    }
}