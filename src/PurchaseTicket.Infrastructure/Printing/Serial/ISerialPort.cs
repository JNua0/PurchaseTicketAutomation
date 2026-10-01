namespace PurchaseTicket.Infrastructure.Printing.Serial;

public interface ISerialPort : IDisposable
{
    bool IsOpen { get; }

    void Open();

    void Write(string text);

    void Close();
}