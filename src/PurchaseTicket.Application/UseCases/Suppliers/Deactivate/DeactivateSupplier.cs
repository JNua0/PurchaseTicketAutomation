using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.Suppliers.Deactivate;

public class DeactivateSupplier
{
    private readonly ISupplierRepository _repository;

    public DeactivateSupplier(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(int supplierId)
    {
        var supplier = await _repository.GetByIdAsync(supplierId);

        if (supplier is null)
        {
            throw new InvalidOperationException(
                "Supplier not found.");
        }

        supplier.Deactivate();

        await _repository.UpdateAsync(supplier);
    }
}