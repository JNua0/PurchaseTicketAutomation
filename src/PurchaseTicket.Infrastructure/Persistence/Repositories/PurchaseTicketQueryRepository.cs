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
        DateTime? weighedTo = null,
        bool oldestFirst = false,
        int? supplierId = null,
        int? materialId = null)
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

        if (supplierId.HasValue)
        {
            query = query.Where(x =>
                x.Ticket.SupplierId == supplierId.Value);
        }

        if (materialId.HasValue)
        {
            query = query.Where(x =>
                x.Ticket.MaterialId == materialId.Value);
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

        // El Id desempata dos pesajes en el mismo instante, para que la
        // paginacion no repita ni salte filas
        var ordered = oldestFirst
            ? query
                .OrderBy(x => x.Ticket.DepartureAt ?? x.Ticket.CheckInAt)
                .ThenBy(x => x.Ticket.Id)
            : query
                .OrderByDescending(x => x.Ticket.DepartureAt ?? x.Ticket.CheckInAt)
                .ThenByDescending(x => x.Ticket.Id);

        var items =
            await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PurchaseTicketListItem(
                    x.Ticket.Id,
                    x.Ticket.TicketNumber,
                    x.Ticket.CheckInAt,
                    x.Ticket.DepartureAt,
                    x.Ticket.SupplierId,
                    x.Ticket.MaterialId,
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

    public async Task<DayWeighingTotals> GetDayTotalsAsync(DateTime from, DateTime to)
    {
        // Mismo criterio de "ultimo pesaje" que el filtro de fechas de GetPagedAsync
        var totals = await _context.PurchaseTickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.Status != TicketStatus.Cancelled &&
                (ticket.DepartureAt ?? ticket.CheckInAt) >= from &&
                (ticket.DepartureAt ?? ticket.CheckInAt) < to)
            .GroupBy(_ => 1)
            .Select(group => new DayWeighingTotals(
                group.Count(),
                group.Count(ticket => ticket.WeighingType == WeighingType.Conventional),
                group.Count(ticket => ticket.WeighingType == WeighingType.Single),
                group.Sum(ticket => ticket.NetWeight) ?? 0m))
            .SingleOrDefaultAsync();

        // Sin tickets en el rango no hay grupo
        return totals ?? new DayWeighingTotals(0, 0, 0, 0m);
    }

    public async Task<PurchaseTicketDetail?> GetDetailAsync(int id)
    {
        var query =
            from ticket in _context.PurchaseTickets.AsNoTracking()
            join supplier in _context.Suppliers.AsNoTracking()
                on ticket.SupplierId equals supplier.Id
            join material in _context.Materials.AsNoTracking()
                on ticket.MaterialId equals material.Id
            where ticket.Id == id
            select new PurchaseTicketDetail(
                ticket.Id,
                ticket.TicketNumber,
                ticket.Status,
                ticket.WeighingType,
                ticket.CheckInAt,
                ticket.DepartureAt,
                ticket.SupplierId,
                supplier.Name,
                ticket.MaterialId,
                material.Name,
                ticket.LicensePlate,
                ticket.Transporter,
                ticket.GrossWeight,
                ticket.TareWeight,
                ticket.NetWeight,
                ticket.Discount,
                ticket.DiscountWeight,
                ticket.NetWeightAfterDiscount,
                ticket.PricePerKg,
                ticket.Amount,
                ticket.UpdatedAt);

        return await query.SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<CatalogOption>> GetSuppliersInUseAsync()
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(supplier => _context.PurchaseTickets
                .Any(ticket => ticket.SupplierId == supplier.Id))
            .OrderBy(supplier => supplier.Name)
            .Select(supplier => new CatalogOption(
                supplier.Id,
                supplier.Name,
                supplier.IsActive))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CatalogOption>> GetMaterialsInUseAsync()
    {
        return await _context.Materials
            .AsNoTracking()
            .Where(material => _context.PurchaseTickets
                .Any(ticket => ticket.MaterialId == material.Id))
            .OrderBy(material => material.Name)
            .Select(material => new CatalogOption(
                material.Id,
                material.Name,
                material.IsActive))
            .ToListAsync();
    }
}