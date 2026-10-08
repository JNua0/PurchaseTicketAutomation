namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;

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
    decimal? Discount = null,
    decimal? PricePerKg = null);