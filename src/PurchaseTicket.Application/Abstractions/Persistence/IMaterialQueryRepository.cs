using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;

namespace PurchaseTicket.Application.Abstractions.Persistence;

public interface IMaterialQueryRepository
{
    Task<PagedResult<MaterialListItem>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null);
}