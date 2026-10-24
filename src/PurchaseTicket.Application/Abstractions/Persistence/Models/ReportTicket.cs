using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

// Un ticket que cuenta en el reporte: completado o pendiente de importe, con
// peso neto. LastWeighingAt va en UTC; lo que aun no tiene, en null
public sealed record ReportTicket(
    string TicketNumber,
    DateTime LastWeighingAt,
    int SupplierId,
    string SupplierName,
    bool IsSupplierActive,
    int MaterialId,
    string MaterialName,
    bool IsMaterialActive,
    string? LicensePlate,
    string Transporter,
    WeighingType WeighingType,
    TicketStatus Status,
    decimal NetWeight,
    int? Discount,
    decimal? NetWeightAfterDiscount,
    decimal? PricePerKg,
    decimal? Amount);
