using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Get;

public record GetPurchaseTicketsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    TicketStatus? Status = null,
    WeighingType? WeighingType = null);