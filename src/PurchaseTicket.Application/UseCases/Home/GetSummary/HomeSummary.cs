using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;

namespace PurchaseTicket.Application.UseCases.Home.GetSummary;

// Lo que muestra Inicio. Las listas traen solo los mas antiguos, hasta
// GetHomeSummary.ListSize; los contadores son el total real
public sealed record HomeSummary(
    int InPlantCount,
    IReadOnlyList<PurchaseTicketDto> InPlant,
    int AmountPendingCount,
    IReadOnlyList<PurchaseTicketDto> AmountPending,
    DayWeighingTotals Today);
