using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Abstractions.Persistence;

public interface IMaterialRepository
{
    Task AddAsync(Material material);
    Task<bool> ExistsByNameAsync(
    string name,
    int? excludeMaterialId = null);
    Task<Material?> GetByIdAsync(int id);
    Task UpdateAsync(Material material);
    Task<IReadOnlyList<Material>> GetAllAsync();
    Task<IReadOnlyList<Material>> SearchByNameAsync(string name);
}