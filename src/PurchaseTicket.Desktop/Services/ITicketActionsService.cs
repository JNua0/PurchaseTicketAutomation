namespace PurchaseTicket.Desktop.Services;

// Cancelar y reimprimir, iguales en Pesaje, Importes y Tickets. Cada uno
// muestra sus avisos; la pantalla solo se encarga de recargar su lista
public interface ITicketActionsService
{
    // Pide confirmacion antes de cancelar
    Task<CancelTicketOutcome> CancelAsync(int ticketId, string ticketNumber);

    Task ReprintAsync(int ticketId, string ticketNumber);
}