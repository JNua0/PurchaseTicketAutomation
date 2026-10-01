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
        return new SerialPortAdapter(_options);
    }
}