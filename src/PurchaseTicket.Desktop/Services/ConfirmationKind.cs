namespace PurchaseTicket.Desktop.Services;

// Tipo de accion que se confirma; decide el color del icono y del boton
public enum ConfirmationKind
{
    // Rojo: desactivar o cancelar
    Danger,

    // Verde y azul: activar
    Positive
}
