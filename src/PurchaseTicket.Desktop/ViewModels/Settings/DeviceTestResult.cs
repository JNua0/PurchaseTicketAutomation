namespace PurchaseTicket.Desktop.ViewModels.Settings;

// Resultado de "Imprimir prueba" o "Leer peso", listo para mostrar
public sealed record DeviceTestResult(
    bool Succeeded,
    string Message);