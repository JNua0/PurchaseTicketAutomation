namespace PurchaseTicket.Desktop.Services;

// Avisos breves que confirman una accion o informan un problema
public interface INotificationService
{
    void ShowSuccess(string message);

    void ShowError(string message);
}