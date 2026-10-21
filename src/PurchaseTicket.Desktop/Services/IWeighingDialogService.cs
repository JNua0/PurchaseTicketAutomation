using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.Services;

public interface IWeighingDialogService
{
    // Null si se cerro sin registrar
    WeighingRegistration? ShowCreateDialog();
}