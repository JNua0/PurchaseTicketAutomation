using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Abstractions.Persistence;

public interface ISupplierRepository
{
    Task AddAsync(Supplier supplier);
    Task<bool> ExistsByNameAsync(string name, int? excludeSupplierId = null);
    Task<Supplier?> GetByIdAsync(int id);
    Task UpdateAsync(Supplier supplier);
    Task<IReadOnlyList<Supplier>> GetActiveAsync();
}