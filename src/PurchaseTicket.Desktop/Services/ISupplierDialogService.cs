namespace PurchaseTicket.Desktop.Services;

public interface ISupplierDialogService
{
    bool ShowCreateDialog();

    bool ShowEditDialog(
        int supplierId,
        string name,
        string? phoneNumber);
}