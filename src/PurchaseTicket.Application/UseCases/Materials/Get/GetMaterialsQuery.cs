namespace PurchaseTicket.Application.UseCases.Materials.Get;

public record GetMaterialsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null);