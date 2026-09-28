using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Infrastructure.Persistence.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly ApplicationDbContext _context;

    public SupplierRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Supplier supplier)
    {
        await _context.Suppliers.AddAsync(supplier);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        int? excludeSupplierId = null)
    {
        return await _context.Suppliers.AnyAsync(
            supplier =>
                supplier.Name == name &&
                (!excludeSupplierId.HasValue ||
                 supplier.Id != excludeSupplierId.Value));
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(supplier => supplier.Id == id);
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Supplier>> GetAllAsync()
    {
        return await _context.Suppliers
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Supplier>> SearchByNameAsync(string name)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(supplier =>
                EF.Functions.ILike(
                    supplier.Name,
                    $"%{name}%"))
            .ToListAsync();
    }
}