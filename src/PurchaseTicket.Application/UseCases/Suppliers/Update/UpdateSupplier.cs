using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.UseCases.Suppliers.Update;

public class UpdateSupplier
{
    private readonly ISupplierRepository _repository;

    public UpdateSupplier(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(
        UpdateSupplierCommand command)
    {
        var supplier = await _repository.GetByIdAsync(
            command.SupplierId);

        if (supplier is null)
        {
            throw new InvalidOperationException(
                "Supplier not found.");
        }

        var normalizedName = Supplier.NormalizeName(
            command.Name);

        if (await _repository.ExistsByNameAsync(
            normalizedName,
            command.SupplierId))
        {
            throw new InvalidOperationException(
                "A supplier with the same name already exists.");
        }

        supplier.UpdateName(command.Name);
        supplier.UpdatePhoneNumber(command.PhoneNumber);

        await _repository.UpdateAsync(supplier);
    }


}