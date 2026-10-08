using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Infrastructure.Persistence.Repositories;

public class PurchaseTicketQueryRepository
    : IPurchaseTicketQueryRepository
{
    private readonly ApplicationDbContext _context;

    public PurchaseTicketQueryRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PurchaseTicketListItem>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        TicketStatus? status = null,
        WeighingType? weighingType = null)
    {
        var query =
            from ticket in _context.PurchaseTickets.AsNoTracking()
            join supplier in _context.Suppliers.AsNoTracking()
                on ticket.SupplierId equals supplier.Id
            join material in _context.Materials.AsNoTracking()
                on ticket.MaterialId equals material.Id
            select new
            {
                Ticket = ticket,
                SupplierName = supplier.Name,
                MaterialName = material.Name
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(x =>
                x.Ticket.TicketNumber.Contains(value) ||
                (x.Ticket.LicensePlate != null &&
                 x.Ticket.LicensePlate.Contains(value)) ||
                x.Ticket.Transporter.Contains(value) ||
                x.SupplierName.Contains(value) ||
                x.MaterialName.Contains(value));
        }

        if (status.HasValue)
        {
            query = query.Where(x =>
                x.Ticket.Status == status.Value);
        }

        if (weighingType.HasValue)
        {
            query = query.Where(x =>
                x.Ticket.WeighingType == weighingType.Value);
        }

        var totalCount =
            await query.CountAsync();

        var items =
            await query
                .OrderByDescending(x => x.Ticket.CheckInAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PurchaseTicketListItem(
                    x.Ticket.TicketNumber,
                    x.Ticket.CheckInAt,
                    x.SupplierName,
                    x.MaterialName,
                    x.Ticket.LicensePlate,
                    x.Ticket.Transporter,
                    x.Ticket.NetWeightAfterDiscount,
                    x.Ticket.Amount,
                    x.Ticket.WeighingType,
                    x.Ticket.Status))
                .ToListAsync();

        return new PagedResult<PurchaseTicketListItem>(
            items,
            page,
            pageSize,
            totalCount);
    }
}