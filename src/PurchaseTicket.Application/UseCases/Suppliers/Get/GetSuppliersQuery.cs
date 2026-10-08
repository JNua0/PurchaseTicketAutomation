namespace PurchaseTicket.Application.UseCases.Suppliers.Get;

public record GetSuppliersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null);