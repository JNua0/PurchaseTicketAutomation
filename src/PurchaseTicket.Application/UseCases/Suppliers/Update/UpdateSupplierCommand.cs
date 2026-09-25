namespace PurchaseTicket.Application.UseCases.Suppliers.Update;

public record UpdateSupplierCommand(
    int SupplierId,
    string Name,
    string? PhoneNumber);