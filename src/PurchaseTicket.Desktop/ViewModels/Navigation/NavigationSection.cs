namespace PurchaseTicket.Desktop.ViewModels.Navigation;

// Grupo de opciones del menu con su titulo, como "OPERACIÓN"
public sealed record NavigationSection(
    string Title,
    IReadOnlyList<NavigationItem> Items);
