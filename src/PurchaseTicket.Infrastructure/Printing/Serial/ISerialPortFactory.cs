using PurchaseTicket.Infrastructure.Communication.Serial;

namespace PurchaseTicket.Infrastructure.Printing.Serial;

public interface ISerialPortFactory
{
    ISerialPort Create();
}