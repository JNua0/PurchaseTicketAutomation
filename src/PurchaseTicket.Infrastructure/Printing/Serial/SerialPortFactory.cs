using System.IO.Ports;
using PurchaseTicket.Infrastructure.Communication.Serial;

namespace PurchaseTicket.Infrastructure.Printing.Serial;

public sealed class SerialPortFactory : ISerialPortFactory
{
    private readonly SerialPrinterOptions _options;

    public SerialPortFactory(SerialPrinterOptions options)
    {
        _options = options;
    }

    public ISerialPort Create()
    {
        var serialPort = new SerialPort
        {
            PortName = _options.PortName,
            BaudRate = _options.BaudRate,
            DataBits = _options.DataBits,
            Parity = _options.Parity,
            StopBits = _options.StopBits,
            Handshake = _options.Handshake
        };

        return new SerialPortAdapter(serialPort);
    }
}