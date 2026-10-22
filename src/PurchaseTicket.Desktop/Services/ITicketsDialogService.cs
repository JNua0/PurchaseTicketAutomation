using PurchaseTicket.Application.Abstractions.Persistence.Models;

namespace PurchaseTicket.Desktop.Services;

public interface ITicketsDialogService
{
    // True si el ticket cambio mientras estaba abierto, para recargar la lista
    bool ShowDetailDialog(int ticketId);

    // True si se guardo la correccion
    bool ShowCorrectDialog(PurchaseTicketDetail ticket);
}