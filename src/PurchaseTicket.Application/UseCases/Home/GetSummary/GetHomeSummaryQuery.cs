namespace PurchaseTicket.Application.UseCases.Home.GetSummary;

// El dia local de la PC en UTC: el inicio se incluye y el fin no,
// igual que el periodo de la pantalla Tickets
public sealed record GetHomeSummaryQuery(
    DateTime DayStartUtc,
    DateTime DayEndUtc);
