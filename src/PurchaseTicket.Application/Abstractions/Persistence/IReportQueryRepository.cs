using PurchaseTicket.Application.Abstractions.Persistence.Models;

namespace PurchaseTicket.Application.Abstractions.Persistence;

public interface IReportQueryRepository
{
    // Completados y pendientes de importe cuyo ultimo pesaje cae en
    // [fromUtc, toUtc), del mas antiguo al mas reciente
    Task<IReadOnlyList<ReportTicket>> GetTicketsAsync(DateTime fromUtc, DateTime toUtc);
}
