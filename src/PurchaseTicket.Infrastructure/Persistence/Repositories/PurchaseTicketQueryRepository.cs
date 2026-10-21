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
        WeighingType? weighingType = null,
        DateTime? weighedFrom = null,
        DateTime? weighedTo = null)
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
            var pattern = $"%{search.Trim()}%";

            // ILike no distingue mayusculas, igual que en proveedores y materiales
            query = query.Where(x =>
                EF.Functions.ILike(x.Ticket.TicketNumber, pattern) ||
                (x.Ticket.LicensePlate != null &&
                 EF.Functions.ILike(x.Ticket.LicensePlate, pattern)) ||
                EF.Functions.ILike(x.Ticket.Transporter, pattern) ||
                EF.Functions.ILike(x.SupplierName, pattern) ||
                EF.Functions.ILike(x.MaterialName, pattern));
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

        // Ultimo pesaje: la salida en convencionales con tara y la
        // entrada en el resto (pesaje unico o vehiculo aun en planta)
        if (weighedFrom.HasValue)
        {
            var from = weighedFrom.Value;

            query = query.Where(x =>
                (x.Ticket.DepartureAt ?? x.Ticket.CheckInAt) >= from);
        }

        if (weighedTo.HasValue)
        {
            var to = weighedTo.Value;

            query = query.Where(x =>
                (x.Ticket.DepartureAt ?? x.Ticket.CheckInAt) < to);
        }

        var totalCount =
            await query.CountAsync();

        var items =
            await query
                .OrderByDescending(x =>
                    x.Ticket.DepartureAt ?? x.Ticket.CheckInAt)
                .ThenByDescending(x => x.Ticket.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PurchaseTicketListItem(
                    x.Ticket.Id,
                    x.Ticket.TicketNumber,
                    x.Ticket.CheckInAt,
                    x.Ticket.DepartureAt,
                    x.SupplierName,
                    x.MaterialName,
                    x.Ticket.LicensePlate,
                    x.Ticket.Transporter,
                    x.Ticket.GrossWeight,
                    x.Ticket.NetWeight,
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