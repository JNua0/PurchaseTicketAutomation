using System.IO.Ports;

namespace PurchaseTicket.Infrastructure.Communication.Serial;

public sealed class SerialPortAdapter : ISerialPort
{
    private readonly SerialPort _serialPort;

    public SerialPortAdapter(SerialPort serialPort)
    {
        _serialPort = serialPort;
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

    public string ReadLine()
    {
        return _serialPort.ReadLine();
    }

    public void DiscardInBuffer()
    {
        _serialPort.DiscardInBuffer();
    }

    public void Close()
    {
        _serialPort.Close();
    }

    public void Dispose()
    {
        _serialPort.Dispose();
    }

    public int ReadTimeout
    {
        get => _serialPort.ReadTimeout;
        set => _serialPort.ReadTimeout = value;
    }
}