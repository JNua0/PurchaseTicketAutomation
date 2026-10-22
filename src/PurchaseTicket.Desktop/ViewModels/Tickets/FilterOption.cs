namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Opcion de un filtro; Value en null es "Todos". Es un record para que el
// selector conserve la opcion elegida al recargar la lista, comparando valores
public sealed record FilterOption<T>(
    T? Value,
    string Label)
    where T : struct;