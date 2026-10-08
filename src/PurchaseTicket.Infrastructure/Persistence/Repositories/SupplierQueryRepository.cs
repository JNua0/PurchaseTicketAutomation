using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;

namespace PurchaseTicket.Infrastructure.Persistence.Repositories;

public class SupplierQueryRepository
    : ISupplierQueryRepository
{
    private readonly ApplicationDbContext _context;

    public SupplierQueryRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SupplierListItem>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null)
    {
        var query =
            _context.Suppliers
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(supplier =>
                EF.Functions.ILike(
                    supplier.Name,
                    $"%{value}%"));
        }

        if (isActive.HasValue)
            query = query.Where(supplier => supplier.IsActive == isActive.Value);

        var totalCount =
            await query.CountAsync();

        var items =
            await query
                .OrderBy(supplier => supplier.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(supplier => new SupplierListItem(
                    supplier.Id,
                    supplier.Name,
                    supplier.PhoneNumber,
                    supplier.IsActive))
                .ToListAsync();

        return new PagedResult<SupplierListItem>(
            items,
            page,
            pageSize,
            totalCount);
    }
}