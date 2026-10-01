using System.IO.Ports;

namespace PurchaseTicket.Infrastructure.Printing.Serial;

public sealed class SerialPortAdapter : ISerialPort
{
    private readonly SerialPort _serialPort;

    public SerialPortAdapter(SerialPrinterOptions options)
    {
        _serialPort = new SerialPort
        {
            PortName = options.PortName,
            BaudRate = options.BaudRate,
            DataBits = options.DataBits,
            Parity = options.Parity,
            StopBits = options.StopBits,
            Handshake = options.Handshake
        };
    }

    public bool IsOpen => _serialPort.IsOpen;

    public void Open()
    {
        _serialPort.Open();
    }

    public void Write(string text)
    {
        _serialPort.Write(text);
    }

    public void Close()
    {
        _serialPort.Close();
    }

    public void Dispose()
    {
        _serialPort.Dispose();
    }
}