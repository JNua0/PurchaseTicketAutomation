using PurchaseTicket.Application.Abstractions.Persistence.Models;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetFilterOptions;

public record TicketFilterOptions(
    IReadOnlyList<CatalogOption> Suppliers,
    IReadOnlyList<CatalogOption> Materials);