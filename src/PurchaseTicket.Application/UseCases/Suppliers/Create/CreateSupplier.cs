using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.UseCases.Suppliers.Create;

public class CreateSupplier
{
    private readonly ISupplierRepository _repository;

    public CreateSupplier(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(
        CreateSupplierCommand command)
    {
        var supplier = new Supplier(
            command.Name,
            command.PhoneNumber);

        if (await _repository.ExistsByNameAsync(supplier.Name))
        {
            throw new NameAlreadyExistsException(
                "Ya existe un proveedor registrado con ese nombre.");
        }

        await _repository.AddAsync(supplier);
    }
}