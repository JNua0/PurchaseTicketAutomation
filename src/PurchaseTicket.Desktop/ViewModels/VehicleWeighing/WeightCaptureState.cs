namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Momento en que esta la captura de un peso
public enum WeightCaptureState
{
    // Aun no se lee ni se captura nada
    Empty,

    // Esperando el peso estable del indicador
    Reading,

    // El indicador devolvio un peso
    Read,

    // La lectura fallo; se puede reintentar o capturar a mano
    Error,

    // El operador escribe el peso
    Manual
}