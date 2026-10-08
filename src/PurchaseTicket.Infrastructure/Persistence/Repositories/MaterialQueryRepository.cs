using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;

namespace PurchaseTicket.Infrastructure.Persistence.Repositories;

public class MaterialQueryRepository : IMaterialQueryRepository
{
    private readonly ApplicationDbContext _context;

    public MaterialQueryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MaterialListItem>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null)
    {
        var query = _context.Materials.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(material =>
                EF.Functions.ILike(
                    material.Name,
                    $"%{value}%"));
        }

        if (isActive.HasValue)
            query = query.Where(material => material.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var items =
            await query
                .OrderBy(material => material.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(material => new MaterialListItem(
                    material.Id,
                    material.Name,
                    material.IsActive))
                .ToListAsync();

        return new PagedResult<MaterialListItem>(
            items,
            page,
            pageSize,
            totalCount);
    }
}