using PurchaseTicket.Application.UseCases.Materials.Get;

namespace PurchaseTicket.Desktop.Services;

public interface IMaterialDialogService
{
    // Mostrar formulario para registrar un material. Devuelven el
    // nombre guardado, o null si se cancelo
    string? ShowCreateDialog();

    // Mostrar formulario para editar un material
    string? ShowEditDialog(MaterialDto material);
}
