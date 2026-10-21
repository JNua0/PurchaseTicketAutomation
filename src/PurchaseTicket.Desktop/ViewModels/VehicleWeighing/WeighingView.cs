namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Vistas de la pantalla de pesaje; cada ticket aparece solo en una
public enum WeighingView
{
    // Convencionales que esperan su tara, de cualquier fecha
    InPlant,

    // Pesajes unicos de hoy que aun no tienen importe
    SinglesToday
}