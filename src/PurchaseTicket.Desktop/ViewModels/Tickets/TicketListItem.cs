using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Fila de la pantalla Tickets. La fecha llega en UTC y aqui se pasa a hora local
public sealed record TicketListItem(
    int Id,
    string TicketNumber,
    DateTime WeighedAt,
    string SupplierName,
    string MaterialName,
    WeighingType WeighingType,
    decimal? NetWeight,
    decimal? Amount,
    TicketStatus Status)
{
    // Propiedades

    public string WeighingTypeText => TicketStatusDisplay.WeighingTypeText(WeighingType);

    public string StatusText => TicketStatusDisplay.Text(Status);

    public StatusKind StatusKind => TicketStatusDisplay.Kind(Status);

    // Metodos publicos

    public static TicketListItem From(
        PurchaseTicketDto ticket,
        TimeProvider clock)
    {
        // Ultimo pesaje: la salida si ya hubo tara, si no la entrada
        DateTime weighedUtc = ticket.DepartureAt ?? ticket.CheckInAt;

        if (weighedUtc.Kind != DateTimeKind.Utc)
            weighedUtc = DateTime.SpecifyKind(weighedUtc, DateTimeKind.Utc);

        return new TicketListItem(
            ticket.Id,
            ticket.TicketNumber,
            TimeZoneInfo.ConvertTimeFromUtc(weighedUtc, clock.LocalTimeZone),
            ticket.SupplierName,
            ticket.MaterialName,
            ticket.WeighingType,
            ticket.NetWeight,
            ticket.Amount,
            ticket.Status);
    }
}