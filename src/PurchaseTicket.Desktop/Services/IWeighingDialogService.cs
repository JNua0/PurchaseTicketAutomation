using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.Services;

public interface IWeighingDialogService
{
    // Null si se cerro sin registrar
    WeighingRegistration? ShowCreateDialog();

    // True si se guardaron cambios
    bool ShowEditDialog(WeighingListItem ticket);

    // Null si se cerro sin registrar la tara
    TareRegistration? ShowRegisterTareDialog(WeighingListItem ticket);
}