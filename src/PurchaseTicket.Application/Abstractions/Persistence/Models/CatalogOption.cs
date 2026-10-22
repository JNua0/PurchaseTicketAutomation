namespace PurchaseTicket.Application.Abstractions.Persistence.Models;

// Proveedor o material como opcion de un filtro. IsActive permite marcar
// los inactivos, que siguen apareciendo porque tienen tickets
public record CatalogOption(
    int Id,
    string Name,
    bool IsActive);