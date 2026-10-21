namespace PurchaseTicket.Desktop.Components.Actions;

// Variante de color de RowActionButton segun lo que hace la accion
public enum RowActionVariant
{
    // Azul: editar
    Edit,

    // Gris: acciones que no cambian datos, como reimprimir
    Neutral,

    // Rojo: acciones destructivas, como cancelar
    Danger,

    // Verde: activar
    Success
}