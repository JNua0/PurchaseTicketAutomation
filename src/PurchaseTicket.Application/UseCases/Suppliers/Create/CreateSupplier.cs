using PurchaseTicket.Application.Abstractions.Persistence;
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
            throw new InvalidOperationException(
                "A supplier with the same name already exists.");
        }

        await _repository.AddAsync(supplier);
    }
}