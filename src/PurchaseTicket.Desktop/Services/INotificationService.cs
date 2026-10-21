namespace PurchaseTicket.Desktop.Services;

// Avisos que confirman una accion o informan un problema.
// Exito e informacion se cierran solos; advertencia y error los cierra el operador
public interface INotificationService
{
    void ShowSuccess(string message);

    void ShowInfo(string message);

    // Algo se completo, pero con un pendiente, como un ticket guardado sin imprimir
    void ShowWarning(string message);

    void ShowError(string message);
}