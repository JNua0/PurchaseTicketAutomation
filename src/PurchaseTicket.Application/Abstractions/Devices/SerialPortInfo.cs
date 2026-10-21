namespace PurchaseTicket.Application.Abstractions.Devices;

// Puerto serie que Windows detecta en el equipo. Description es el nombre del
// dispositivo, como "USB-SERIAL CH340"; puede faltar si Windows no lo informa
public sealed record SerialPortInfo(
    string Name,
    string? Description);