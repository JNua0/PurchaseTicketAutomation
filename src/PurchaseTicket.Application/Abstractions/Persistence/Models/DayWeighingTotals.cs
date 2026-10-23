namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

// Tickets cuyo ultimo pesaje cae en un rango, sin contar cancelados.
// NetWeight suma solo los que ya tienen peso neto: un convencional que sigue
// en planta cuenta como pesaje del dia, pero todavia no tiene kilos
public sealed record DayWeighingTotals(
    int TicketCount,
    int ConventionalCount,
    int SingleCount,
    decimal NetWeight);
