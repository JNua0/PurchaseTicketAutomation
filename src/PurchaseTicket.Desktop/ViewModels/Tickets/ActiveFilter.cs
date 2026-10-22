namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Etiqueta de un filtro activo, como "Estado: Completado", que se muestra
// con los filtros cerrados para que la tabla no quede filtrada en silencio
public sealed record ActiveFilter(
    ActiveFilterKind Kind,
    string Label,
    string Value);