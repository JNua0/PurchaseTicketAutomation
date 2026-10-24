namespace PurchaseTicket.Desktop.Services;

public interface IReportsDialogService
{
    // Ruta elegida para guardar, o null si se cancelo. extension es "xlsx" o "pdf"
    string? ShowSaveDialog(string suggestedName, string extension);

    // Abre el archivo con el programa predeterminado, como Excel
    void OpenFile(string path);
}
