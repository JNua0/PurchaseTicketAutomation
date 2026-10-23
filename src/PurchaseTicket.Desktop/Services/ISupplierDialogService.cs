using PurchaseTicket.Application.UseCases.Suppliers.Get;

namespace PurchaseTicket.Desktop.Services;

public interface ISupplierDialogService
{
    bool ShowCreateDialog();

    bool ShowEditDialog(SupplierDto supplier);
}