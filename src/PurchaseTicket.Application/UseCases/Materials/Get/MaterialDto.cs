namespace PurchaseTicket.Application.UseCases.Materials.Get;

public record MaterialDto(
    int Id,
    string Name,
    bool IsActive);