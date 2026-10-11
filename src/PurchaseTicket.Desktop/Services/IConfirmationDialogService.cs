namespace PurchaseTicket.Desktop.Services;

public interface IConfirmationDialogService
{
    bool Confirm(string title, string message);
}