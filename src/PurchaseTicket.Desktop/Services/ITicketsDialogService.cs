namespace PurchaseTicket.Desktop.Services;

public interface ITicketsDialogService
{
    // True si el ticket cambio mientras estaba abierto, para recargar la lista
    bool ShowDetailDialog(int ticketId);
}