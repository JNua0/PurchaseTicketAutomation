using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.Suppliers.GetActive;

public class GetActiveSuppliers
{
    private readonly ISupplierRepository _repository;

    public GetActiveSuppliers(
        ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<SupplierOptionDto>> ExecuteAsync()
    {
        var suppliers =
            await _repository.GetActiveAsync();

        return suppliers
            .Select(supplier => new SupplierOptionDto(
                supplier.Id,
                supplier.Name))
            .ToList();
    }
}