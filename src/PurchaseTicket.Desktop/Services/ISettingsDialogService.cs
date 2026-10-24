using PurchaseTicket.Desktop.Configuration;

namespace PurchaseTicket.Desktop.Services;

public interface ISettingsDialogService
{
    // Lo guardado, o null si se cerro con Cancelar
    TicketHeaderSettings? ShowTicketHeaderDialog(TicketHeaderSettings current);

    // La carpeta elegida, o null si se cerro sin elegir
    string? ShowBackupFolderDialog(string currentFolder);

    // Abre la carpeta en el Explorador de Windows
    void OpenFolder(string folder);
}