using PurchaseTicket.Domain.ValueObjects;

namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

// Una correccion del historial de un ticket; la fecha va en UTC
public record TicketCorrectionItem(
    int Id,
    DateTime CorrectedAt,
    string? Reason,
    IReadOnlyList<TicketFieldChange> Changes);
