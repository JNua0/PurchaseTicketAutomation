using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Infrastructure.Persistence.Repositories;

public class ReportQueryRepository
    : IReportQueryRepository
{
    private readonly ApplicationDbContext _context;

    public ReportQueryRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ReportTicket>> GetTicketsAsync(DateTime fromUtc, DateTime toUtc)
    {
        // Mismo "ultimo pesaje" que Tickets e Inicio: la salida, o la entrada
        // si no hay salida. Sin cancelados ni vehiculos en planta
        var query =
            from ticket in _context.PurchaseTickets.AsNoTracking()
            join supplier in _context.Suppliers.AsNoTracking()
                on ticket.SupplierId equals supplier.Id
            join material in _context.Materials.AsNoTracking()
                on ticket.MaterialId equals material.Id
            where (ticket.Status == TicketStatus.Completed || ticket.Status == TicketStatus.AmountPending)
                && ticket.NetWeight != null
                && (ticket.DepartureAt ?? ticket.CheckInAt) >= fromUtc
                && (ticket.DepartureAt ?? ticket.CheckInAt) < toUtc
            orderby ticket.DepartureAt ?? ticket.CheckInAt, ticket.Id
            select new ReportTicket(
                ticket.TicketNumber,
                ticket.DepartureAt ?? ticket.CheckInAt,
                ticket.SupplierId,
                supplier.Name,
                supplier.IsActive,
                ticket.MaterialId,
                material.Name,
                material.IsActive,
                ticket.LicensePlate,
                ticket.Transporter,
                ticket.WeighingType,
                ticket.Status,
                ticket.NetWeight!.Value,
                ticket.Discount,
                ticket.NetWeightAfterDiscount,
                ticket.PricePerKg,
                ticket.Amount);

        return await query.ToListAsync();
    }
}
