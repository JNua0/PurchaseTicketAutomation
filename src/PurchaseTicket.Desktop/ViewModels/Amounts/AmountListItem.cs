using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.Amounts;

// Fila de Importes: solo lo que se necesita para cobrar, sin bruto ni tara.
// La fecha llega en UTC desde la base y aqui se pasa a hora local
public sealed record AmountListItem(
    int Id,
    string TicketNumber,
    DateTime WeighedAt,
    string WaitingTime,
    string SupplierName,
    string MaterialName,
    WeighingType WeighingType,
    decimal? NetWeight)
{
    // Propiedades

    public string WeighingTypeText =>
        WeighingType == WeighingType.Conventional
            ? "Convencional"
            : "Un solo pesaje";

    // Metodos publicos

    public static AmountListItem From(
        PurchaseTicketDto ticket,
        TimeProvider clock)
    {
        TimeZoneInfo zone = clock.LocalTimeZone;

        // Fin del pesaje: la salida en convencionales, la unica medicion en el resto
        DateTime weighedUtc = AsUtc(ticket.DepartureAt ?? ticket.CheckInAt);
        DateTime weighedLocal = TimeZoneInfo.ConvertTimeFromUtc(weighedUtc, zone);

        DateTime todayLocal = TimeZoneInfo
            .ConvertTime(clock.GetUtcNow(), zone)
            .Date;

        return new AmountListItem(
            ticket.Id,
            ticket.TicketNumber,
            weighedLocal,
            FormatWaitingTime(todayLocal, weighedLocal.Date),
            ticket.SupplierName,
            ticket.MaterialName,
            ticket.WeighingType,
            ticket.NetWeight);
    }

    // Metodos privados

    // Npgsql ya devuelve UTC; esto solo protege de un DateTime sin tipo
    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    // Por dias de calendario, no por horas: un pesaje de anoche ya es "Hace 1 dia"
    private static string FormatWaitingTime(DateTime today, DateTime weighedDay)
    {
        int days = Math.Max(0, (today - weighedDay).Days);

        return days switch
        {
            0 => "Hoy",
            1 => "Hace 1 día",
            _ => $"Hace {days} días"
        };
    }
}