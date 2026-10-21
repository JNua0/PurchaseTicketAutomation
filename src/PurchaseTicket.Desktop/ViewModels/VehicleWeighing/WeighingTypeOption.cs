using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Opcion del selector de tipo de pesaje, con una linea que la explica
public sealed record WeighingTypeOption(
    WeighingType Value,
    string Title,
    string Description);