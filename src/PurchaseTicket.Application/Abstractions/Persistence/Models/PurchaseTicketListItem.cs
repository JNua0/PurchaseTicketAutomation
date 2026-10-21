using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

public record PurchaseTicketListItem(
    int Id,
    string TicketNumber,
    DateTime CheckInAt,
    DateTime? DepartureAt,
    int SupplierId,
    int MaterialId,
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
