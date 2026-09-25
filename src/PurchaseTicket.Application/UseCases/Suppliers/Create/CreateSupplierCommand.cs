namespace PurchaseTicket.Application.UseCases.Suppliers.Create;

public record CreateSupplierCommand(
    string Name,
    string? PhoneNumber);