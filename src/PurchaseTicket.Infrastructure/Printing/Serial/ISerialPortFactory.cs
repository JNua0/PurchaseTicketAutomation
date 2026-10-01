namespace PurchaseTicket.Infrastructure.Printing.Serial;

public interface ISerialPortFactory
{
    ISerialPort Create();
}