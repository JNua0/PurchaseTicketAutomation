namespace PurchaseTicket.Application.Abstractions.Devices;

// Puertos serie conectados en este momento. La consulta depende del sistema
// operativo, por eso vive en Infrastructure detras de esta interfaz
public interface ISerialPortDiscovery
{
    // Ordenados por numero: COM3, COM5, COM10
    IReadOnlyList<SerialPortInfo> GetPorts();
}