using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.UseCases.Materials.Create;

public class CreateMaterial
{
    private readonly IMaterialRepository _repository;

    public CreateMaterial(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(CreateMaterialCommand command)
    {
        var material = new Material(command.Name);

        if (await _repository.ExistsByNameAsync(material.Name))
        {
            throw new NameAlreadyExistsException(
                "Ya existe un material registrado con ese nombre.");
        }

        await _repository.AddAsync(material);
    }
}