namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

public record SupplierListItem(
    int Id,
    string Name,
    string? PhoneNumber,
    bool IsActive);