using PurchaseTicket.Domain.Enums;

public record PurchaseTicketDto(
    string TicketNumber,
    DateTime CheckInAt,
    string SupplierName,
    string MaterialName,
    string? LicensePlate,
    string Transporter,
    decimal? FinalWeight,
    decimal? Amount,
    WeighingType WeighingType,
    TicketStatus Status);