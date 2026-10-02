namespace PurchaseTicket.Infrastructure.Communication.Serial;

public interface ISerialPort : IDisposable
{
    bool IsOpen { get; }
    int ReadTimeout { get; set; }
    void Open();
    void Write(string text);
    string ReadLine();
    void DiscardInBuffer();
    void Close();
}