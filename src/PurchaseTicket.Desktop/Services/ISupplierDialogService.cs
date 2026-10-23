using PurchaseTicket.Application.UseCases.Suppliers.Get;

namespace PurchaseTicket.Desktop.Services;

public interface ISupplierDialogService
{
    // Devuelven el nombre guardado, o null si se cancelo
    string? ShowCreateDialog();

    string? ShowEditDialog(SupplierDto supplier);
}