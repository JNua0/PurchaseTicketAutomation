using PurchaseTicket.Application.UseCases.Materials.Get;

namespace PurchaseTicket.Desktop.Services;

public interface IMaterialDialogService
{
    // Mostrar formulario para registrar un material
    bool ShowCreateDialog();

    // Mostrar formulario para editar un material
    bool ShowEditDialog(MaterialDto material);
}
