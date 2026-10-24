using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Domain.ValueObjects;

// Un dato que cambio al corregir un ticket. Los valores van como texto
// invariante ("5060", "3.33"); null es un dato vacio, como sin placas.
// IsRecalculated marca lo que no se capturo, como el importe
public sealed record TicketFieldChange(
    TicketField Field,
    string? Before,
    string? After,
    bool IsRecalculated = false);
