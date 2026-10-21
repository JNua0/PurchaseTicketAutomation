namespace PurchaseTicket.Desktop.Services;

// Impresion de tickets desde la interfaz. La usan Nuevo pesaje,
// Registrar tara y la reimpresion desde las listas
public interface ITicketPrintService
{
    // Devuelve null si se imprimio; si no, el motivo para mostrar al usuario
    Task<string?> PrintAsync(int ticketId);
}