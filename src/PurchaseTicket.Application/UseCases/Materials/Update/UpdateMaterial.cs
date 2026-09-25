using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.UseCases.Materials.Update;

public class UpdateMaterial
{
    private readonly IMaterialRepository _repository;

    public UpdateMaterial(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(UpdateMaterialCommand command)
    {
        var material = await _repository.GetByIdAsync(
            command.MaterialId);

        if (material is null)
        {
            throw new InvalidOperationException(
                "Material not found.");
        }

        var normalizedName = Material.NormalizeName(
            command.Name);

        if (await _repository.ExistsByNameAsync(
            normalizedName,
            command.MaterialId))
        {
            throw new InvalidOperationException(
                "A material with the same name already exists.");
        }

        material.UpdateName(command.Name);

        await _repository.UpdateAsync(material);
    }
}