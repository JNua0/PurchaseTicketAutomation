using System.IO.Ports;
using PurchaseTicket.Application.Abstractions.Devices;
using PurchaseTicket.Infrastructure.Communication.Serial;

namespace PurchaseTicket.Infrastructure.Weighing.Serial;

internal sealed class WeightIndicatorSerialPortFactory
    : IWeightIndicatorSerialPortFactory
{
    private readonly WeightIndicatorOptions _options;

    public WeightIndicatorSerialPortFactory(
        WeightIndicatorOptions options)
    {
        _options = options;
    }

    public ISerialPort Create()
    {
        // Sin puerto, SerialPort lanza ArgumentException, que se confundiria
        // con un error interno en lugar de pedir que se configure
        if (string.IsNullOrWhiteSpace(_options.PortName))
        {
            throw new DeviceNotConfiguredException(
                "El indicador de peso no tiene un puerto configurado.");
        }

        var serialPort = new SerialPort
        {
            PortName = _options.PortName,
            BaudRate = _options.BaudRate,
            DataBits = _options.DataBits,
            Parity = _options.Parity,
            StopBits = _options.StopBits,
            Handshake = _options.Handshake,
            ReadTimeout = _options.ReadTimeoutMilliseconds
        };

        return new SerialPortAdapter(serialPort);
    }
}