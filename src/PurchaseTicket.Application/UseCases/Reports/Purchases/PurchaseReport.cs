using System.Globalization;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.Reports.Purchases;

// Reporte de compras de un periodo: totales, desglose por material y por
// proveedor, y los tickets que lo forman. Las fechas van en UTC
public sealed class PurchaseReport
{
    // Campos

    private static readonly StringComparer NameComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("es-MX"), ignoreCase: true);

    // Constructor

    private PurchaseReport(
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyList<ReportTicket> tickets)
    {
        FromUtc = fromUtc;
        ToUtc = toUtc;
        Tickets = tickets;

        CompletedCount = tickets.Count(ticket => ticket.Status == TicketStatus.Completed);
        PendingAmountCount = tickets.Count - CompletedCount;
        NetWeight = tickets.Sum(ticket => ticket.NetWeight);
        NetWeightAfterDiscount = tickets.Sum(ticket => ticket.NetWeightAfterDiscount ?? 0m);
        Amount = tickets.Sum(ticket => ticket.Amount ?? 0m);

        ByMaterial = Group(
            tickets,
            ticket => (ticket.MaterialId, ticket.MaterialName, ticket.IsMaterialActive));

        BySupplier = Group(
            tickets,
            ticket => (ticket.SupplierId, ticket.SupplierName, ticket.IsSupplierActive));
    }

    // Propiedades

    public DateTime FromUtc { get; }

    // Exclusivo: el inicio del dia siguiente al ultimo del periodo
    public DateTime ToUtc { get; }

    public IReadOnlyList<ReportTicket> Tickets { get; }

    public int TicketCount => Tickets.Count;

    public int CompletedCount { get; }

    public int PendingAmountCount { get; }

    // Lo que marco la bascula, de todos los tickets
    public decimal NetWeight { get; }

    // Con descuento: solo los completados lo tienen
    public decimal NetWeightAfterDiscount { get; }

    // Solo los completados
    public decimal Amount { get; }

    public bool IsEmpty => Tickets.Count == 0;

    // De mas a menos kilos; si empatan, por nombre
    public IReadOnlyList<PurchaseReportGroup> ByMaterial { get; }

    public IReadOnlyList<PurchaseReportGroup> BySupplier { get; }

    // Metodos publicos

    public static PurchaseReport Create(
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyList<ReportTicket> tickets)
    {
        return new PurchaseReport(fromUtc, toUtc, tickets);
    }

    // Parte de los kilos del periodo, de 0 a 1
    public decimal ShareOf(PurchaseReportGroup group)
    {
        return NetWeight == 0m ? 0m : group.NetWeight / NetWeight;
    }

    // Metodos privados

    private static IReadOnlyList<PurchaseReportGroup> Group(
        IEnumerable<ReportTicket> tickets,
        Func<ReportTicket, (int Id, string Name, bool IsActive)> keySelector)
    {
        return tickets
            .GroupBy(ticket => keySelector(ticket).Id)
            .Select(group =>
            {
                var key = keySelector(group.First());

                return new PurchaseReportGroup(
                    key.Id,
                    key.Name,
                    key.IsActive,
                    group.Count(),
                    group.Count(ticket => ticket.Status != TicketStatus.Completed),
                    group.Sum(ticket => ticket.NetWeight),
                    group.Sum(ticket => ticket.Amount ?? 0m));
            })
            .OrderByDescending(group => group.NetWeight)
            .ThenBy(group => group.Name, NameComparer)
            .ToList();
    }
}
