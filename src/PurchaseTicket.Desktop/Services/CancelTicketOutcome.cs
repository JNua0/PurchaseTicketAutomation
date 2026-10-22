namespace PurchaseTicket.Desktop.Services;

// Como termino una cancelacion, para que la pantalla decida si recarga
public enum CancelTicketOutcome
{
    // El operador no confirmo: nada cambio
    NotConfirmed,

    Cancelled,

    // El dominio la rechazo, por ejemplo porque otro equipo ya registro el
    // importe: la lista esta desactualizada y conviene recargarla
    Rejected,

    // Error inesperado, ya registrado en el log
    Failed
}