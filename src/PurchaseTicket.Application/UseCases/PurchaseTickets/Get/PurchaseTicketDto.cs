
using PurchaseTicket.Domain.Enums;

public record PurchaseTicketDto(
    int Id,
    string TicketNumber,
    DateTime CheckInAt,
    string SupplierName,
    string MaterialName,
    string? LicensePlate,
    string Transporter,
    decimal? GrossWeight,
    decimal? NetWeight,
    decimal? FinalWeight,
    decimal? Amount,
    WeighingType WeighingType,
    TicketStatus Status);
