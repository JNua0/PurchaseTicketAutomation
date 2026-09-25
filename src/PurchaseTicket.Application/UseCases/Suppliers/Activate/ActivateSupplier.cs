using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.Suppliers.Activate;

public class ActivateSupplier
{
    private readonly ISupplierRepository _repository;

    public ActivateSupplier(ISupplierRepository repository)
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

        supplier.Activate();

        await _repository.UpdateAsync(supplier);
    }
}