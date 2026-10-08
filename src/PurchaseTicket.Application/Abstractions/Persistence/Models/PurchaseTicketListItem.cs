using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

public record PurchaseTicketListItem(
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