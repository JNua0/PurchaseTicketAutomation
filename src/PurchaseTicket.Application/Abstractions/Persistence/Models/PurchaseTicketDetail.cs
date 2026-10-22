using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

// Todos los datos de un ticket con los nombres de proveedor y material.
// Las fechas van en UTC; los datos que el estado aun no tiene, en null
public record PurchaseTicketDetail(
    int Id,
    string TicketNumber,
    TicketStatus Status,
    WeighingType WeighingType,
    DateTime CheckInAt,
    DateTime? DepartureAt,
    int SupplierId,
    string SupplierName,
    int MaterialId,
    string MaterialName,
    string? LicensePlate,
    string Transporter,
    decimal? GrossWeight,
    decimal? TareWeight,
    decimal? NetWeight,
    int? Discount,
    decimal? DiscountWeight,
    decimal? NetWeightAfterDiscount,
    decimal? PricePerKg,
    decimal? Amount,
    DateTime? UpdatedAt);