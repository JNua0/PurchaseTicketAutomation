namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;

// Solo los datos que cambian; Reason es el motivo opcional que queda en el historial
public record CorrectPurchaseTicketCommand(
    int TicketId,
    int? SupplierId = null,
    int? MaterialId = null,
    string? Transporter = null,
    bool ChangeLicensePlate = false,
    string? LicensePlate = null,
    decimal? GrossWeight = null,
    decimal? TareWeight = null,
    decimal? NetWeight = null,
    int? Discount = null,
    decimal? PricePerKg = null,
    string? Reason = null);