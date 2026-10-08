namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

public record MaterialListItem(
    int Id,
    string Name,
    bool IsActive);