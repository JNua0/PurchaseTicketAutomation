namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Periodo del ultimo pesaje en la pantalla Tickets. "Todos" no tiene valor:
// es el null de FilterOption. Solo Custom usa las fechas Desde y Hasta
public enum TicketPeriod
{
    Today,

    Yesterday,

    // Hoy y los seis dias anteriores
    Last7Days,

    // Del dia 1 a hoy
    ThisMonth,

    // El mes anterior completo
    LastMonth,

    Custom
}