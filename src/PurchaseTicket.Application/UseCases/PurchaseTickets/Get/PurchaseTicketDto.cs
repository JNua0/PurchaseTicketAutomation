namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Get;

public record PurchaseTicketDto(
    int Id,
    string TicketNumber,
    DateTime CreatedAt,
    int SupplierId,
    int MaterialId,
    string LicensePlate,
    string DriverName,
    decimal GrossWeight,
    decimal? TareWeight,
    decimal? NetWeight,
    decimal? Discount,
    decimal? DiscountWeight,
    decimal? NetWeightAfterDiscount,
    decimal? PricePerKg,
    decimal? Amount,
    string Status);