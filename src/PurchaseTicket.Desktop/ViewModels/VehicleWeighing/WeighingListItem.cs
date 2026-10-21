using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Fila de la tabla de pesaje con los datos listos para mostrar.
// Las fechas llegan en UTC desde la base y aqui se pasan a hora local
public sealed record WeighingListItem(
    int Id,
    string TicketNumber,
    DateTime CheckInAt,
    DateTime WeighedAt,
    string TimeInPlant,
    string SupplierName,
    string MaterialName,
    string? LicensePlate,
    string Transporter,
    decimal? GrossWeight,
    decimal? NetWeight,
    WeighingType WeighingType,
    TicketStatus Status)
{
    // Propiedades

    // Solo un convencional que sigue en planta espera su tara
    public bool CanRegisterTare =>
        WeighingType == WeighingType.Conventional &&
        Status == TicketStatus.WeighingPending;

    // Metodos publicos

    public static WeighingListItem From(
        PurchaseTicketDto ticket,
        TimeProvider clock)
    {
        TimeZoneInfo zone = clock.LocalTimeZone;

        DateTime checkInUtc = AsUtc(ticket.CheckInAt);

        // Ultimo pesaje: la salida si ya hubo tara, si no la entrada
        DateTime weighedUtc = AsUtc(ticket.DepartureAt ?? ticket.CheckInAt);

        TimeSpan timeInPlant =
            clock.GetUtcNow().UtcDateTime - checkInUtc;

        return new WeighingListItem(
            ticket.Id,
            ticket.TicketNumber,
            TimeZoneInfo.ConvertTimeFromUtc(checkInUtc, zone),
            TimeZoneInfo.ConvertTimeFromUtc(weighedUtc, zone),
            $"En planta: {FormatElapsed(timeInPlant)}",
            ticket.SupplierName,
            ticket.MaterialName,
            ticket.LicensePlate,
            ticket.Transporter,
            ticket.GrossWeight,
            ticket.NetWeight,
            ticket.WeighingType,
            ticket.Status);
    }

    // Metodos privados

    // Npgsql ya devuelve UTC; esto solo protege de un DateTime sin tipo,
    // que se interpretaria como hora local y desfasaria la conversion
    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    // "4 min", "3 h 01 min" o "1 d 2 h": basta para ver quien lleva mas tiempo
    private static string FormatElapsed(TimeSpan elapsed)
    {
        // Un reloj del equipo atrasado respecto al servidor daria negativo
        if (elapsed < TimeSpan.Zero)
            elapsed = TimeSpan.Zero;

        if (elapsed.TotalDays >= 1)
            return $"{(int)elapsed.TotalDays} d {elapsed.Hours} h";

        if (elapsed.TotalHours >= 1)
            return $"{(int)elapsed.TotalHours} h {elapsed.Minutes:00} min";

        return $"{elapsed.Minutes} min";
    }
}