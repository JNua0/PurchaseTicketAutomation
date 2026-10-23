namespace PurchaseTicket.Desktop.Services;

public interface IConfirmationDialogService
{
    // True si el usuario confirma; Esc, la X o Cancelar devuelven false
    bool Confirm(ConfirmationRequest request);
}
