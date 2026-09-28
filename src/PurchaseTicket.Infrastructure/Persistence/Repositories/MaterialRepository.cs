using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Infrastructure.Persistence.Repositories;

public class MaterialRepository : IMaterialRepository
{
    private readonly ApplicationDbContext _context;

    public MaterialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Material material)
    {
        await _context.Materials.AddAsync(material);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        int? excludeMaterialId = null)
    {
        return await _context.Materials.AnyAsync(
            material =>
                material.Name == name &&
                (!excludeMaterialId.HasValue ||
                 material.Id != excludeMaterialId.Value));
    }

    public async Task<Material?> GetByIdAsync(int id)
    {
        return await _context.Materials
            .FirstOrDefaultAsync(material => material.Id == id);
    }

    public async Task UpdateAsync(Material material)
    {
        _context.Materials.Update(material);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Material>> GetAllAsync()
    {
        return await _context.Materials
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Material>> SearchByNameAsync(string name)
    {
        return await _context.Materials
            .AsNoTracking()
            .Where(material =>
                EF.Functions.ILike(
                    material.Name,
                    $"%{name}%"))
            .ToListAsync();
    }
}