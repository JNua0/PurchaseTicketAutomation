namespace PurchaseTicket.Desktop.ViewModels.Settings;

// Opcion del selector de puerto, como "COM5 · USB-SERIAL CH340"
public sealed record PortOption(
    string Name,
    string DisplayName);