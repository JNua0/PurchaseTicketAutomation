namespace PurchaseTicket.Application.Abstractions.Printing;

// Formato fisico del ticket. Lo comparten la impresora, que acomoda el texto,
// y la interfaz, que valida que cada linea del encabezado quepa en el papel
public static class TicketFormat
{
    // Caracteres por linea de la Epson TM-U295 con el papel que se usa
    public const int LineWidth = 35;
}