namespace PurchaseTicket.Desktop.ViewModels.Amounts;

// Resultado de Registrar importe para la lista: el folio y el importe guardado
public sealed record AmountRegistration(
    string TicketNumber,
    decimal Amount);