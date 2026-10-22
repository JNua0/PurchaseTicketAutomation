using PurchaseTicket.Desktop.ViewModels.Amounts;

namespace PurchaseTicket.Desktop.Services;

public interface IAmountDialogService
{
    // Null si se cerro sin registrar el importe
    AmountRegistration? ShowRegisterAmountDialog(AmountListItem ticket);
}