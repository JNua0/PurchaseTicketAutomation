namespace PurchaseTicket.Domain.ValueObjects;

// Resultado del calculo del importe con los valores ya redondeados a dos
// decimales, tal como se guardan (RN-41)
public sealed record AmountBreakdown(
    decimal DiscountWeight,
    decimal NetWeightAfterDiscount,
    decimal Amount);