namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

// Opcion del selector de vista; ShowsCount agrega el contador de vehiculos en planta
public sealed record WeighingViewOption(
    WeighingView Value,
    string Label,
    bool ShowsCount);