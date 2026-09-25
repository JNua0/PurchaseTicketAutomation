using PurchaseTicket.Domain.Entities;

public interface ISupplierRepository
{
    Task AddAsync(Supplier supplier);
    Task<bool> ExistsByNameAsync(
        string name,
        int? excludeSupplierId = null);
    Task<Supplier?> GetByIdAsync(int id);
    Task UpdateAsync(Supplier supplier);
    Task<IReadOnlyList<Supplier>> GetAllAsync();
    Task<IReadOnlyList<Supplier>> SearchByNameAsync(string name);
}