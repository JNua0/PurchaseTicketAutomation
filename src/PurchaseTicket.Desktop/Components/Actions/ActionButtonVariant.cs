namespace PurchaseTicket.Desktop.Components.Actions;

// Variante de color de ActionButton segun lo que hace la accion
public enum ActionButtonVariant
{
    // Contorno gris: cerrar o cancelar sin hacer nada
    Simple,

    // Relleno azul: la accion principal, una por ventana
    Primary,

    // Contorno azul: acciones que no cambian datos, como reimprimir
    Secondary,

    // Contorno rojo: acciones destructivas, como cancelar un ticket
    Danger
}
