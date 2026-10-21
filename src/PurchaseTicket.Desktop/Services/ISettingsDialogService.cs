using PurchaseTicket.Desktop.Configuration;

namespace PurchaseTicket.Desktop.Services;

public interface ISettingsDialogService
{
    // Lo guardado, o null si se cerro con Cancelar
    TicketHeaderSettings? ShowTicketHeaderDialog(TicketHeaderSettings current);
}