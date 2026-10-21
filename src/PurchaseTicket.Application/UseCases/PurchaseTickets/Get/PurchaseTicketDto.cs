using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Get;

public record PurchaseTicketDto(
    int Id,
    string TicketNumber,
    DateTime CheckInAt,
    DateTime? DepartureAt,
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
