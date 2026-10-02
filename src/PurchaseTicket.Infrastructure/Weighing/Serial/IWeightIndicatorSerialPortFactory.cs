using PurchaseTicket.Infrastructure.Communication.Serial;

namespace PurchaseTicket.Infrastructure.Weighing.Serial;

internal interface IWeightIndicatorSerialPortFactory
{
    ISerialPort Create();
}