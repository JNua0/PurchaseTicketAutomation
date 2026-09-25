using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.UseCases.Suppliers.Get;

public class GetSuppliers
{
    private readonly ISupplierRepository _repository;

    public GetSuppliers(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Supplier>> ExecuteAsync(
        string? searchTerm = null)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _repository.GetAllAsync();
        }

        return await _repository.SearchByNameAsync(
            searchTerm.Trim());
    }
}