using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Abstractions.Persistence;

public interface IPurchaseTicketQueryRepository
{
    Task<PagedResult<PurchaseTicketListItem>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        TicketStatus? status = null,
        WeighingType? weighingType = null);
}