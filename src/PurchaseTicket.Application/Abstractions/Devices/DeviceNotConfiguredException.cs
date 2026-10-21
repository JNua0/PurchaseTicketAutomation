namespace PurchaseTicket.Application.Abstractions.Devices;

// El dispositivo no tiene un puerto guardado. Se distingue de un puerto que no
// existe porque al operador se le pide configurarlo, no revisar el cable
public sealed class DeviceNotConfiguredException : Exception
{
    public DeviceNotConfiguredException(string message)
        : base(message)
    {
    }
}