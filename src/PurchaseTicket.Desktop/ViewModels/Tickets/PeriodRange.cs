namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Fechas de un periodo en dias completos de hora local. End es el inicio del
// dia siguiente al ultimo, que las consultas no incluyen. Lo usan Tickets y
// Reportes, para que un mismo periodo cuente los mismos tickets
public static class PeriodRange
{
    // Metodos publicos

    // Personalizado sin fecha deja ese extremo abierto (null)
    public static (DateTime? Start, DateTime? End) ToLocal(
        TicketPeriod period,
        DateTime? fromDate,
        DateTime? toDate,
        DateTime today)
    {
        today = LocalDay(today);

        DateTime firstOfMonth = new(today.Year, today.Month, 1);

        return period switch
        {
            TicketPeriod.Today => (today, today.AddDays(1)),
            TicketPeriod.Yesterday => (today.AddDays(-1), today),
            TicketPeriod.Last7Days => (today.AddDays(-6), today.AddDays(1)),
            TicketPeriod.ThisMonth => (firstOfMonth, today.AddDays(1)),
            TicketPeriod.LastMonth => (firstOfMonth.AddMonths(-1), firstOfMonth),
            TicketPeriod.Custom => (
                fromDate is DateTime from ? LocalDay(from) : null,
                toDate is DateTime to ? LocalDay(to).AddDays(1) : null),
            _ => (null, null)
        };
    }

    // Como los guarda la base
    public static DateTime? ToUtc(DateTime? local, TimeZoneInfo zone)
    {
        return local is DateTime value
            ? TimeZoneInfo.ConvertTimeToUtc(value, zone)
            : null;
    }

    // Metodos privados

    private static DateTime LocalDay(DateTime value)
    {
        return DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
    }
}
