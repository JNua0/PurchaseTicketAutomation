namespace PurchaseTicket.Application.UseCases.Suppliers.Get;

public record SupplierDto(
    int Id,
    string Name,
    string? PhoneNumber,
    bool IsActive);