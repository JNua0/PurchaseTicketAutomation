using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Get;

// WeighedFrom y WeighedTo van en UTC: el inicio se incluye y el fin no,
// asi dos rangos consecutivos (hoy y manana) nunca repiten un ticket.
// OldestFirst ordena por el ultimo pesaje del mas antiguo al mas reciente,
// como en Importes, donde arriba va lo que lleva mas tiempo esperando
public record GetPurchaseTicketsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    TicketStatus? Status = null,
    WeighingType? WeighingType = null,
    DateTime? WeighedFrom = null,
    DateTime? WeighedTo = null,
    bool OldestFirst = false,
    int? SupplierId = null,
    int? MaterialId = null);