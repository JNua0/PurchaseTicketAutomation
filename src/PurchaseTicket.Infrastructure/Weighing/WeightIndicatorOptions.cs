using System.IO.Ports;

namespace PurchaseTicket.Infrastructure.Weighing;

public sealed class WeightIndicatorOptions
{
    public string PortName { get; init; } = string.Empty;

    public int BaudRate { get; init; } = 9600;

    public int DataBits { get; init; } = 8;

    public Parity Parity { get; init; } = Parity.None;

    public StopBits StopBits { get; init; } = StopBits.One;

    public Handshake Handshake { get; init; } = Handshake.None;

    public int ReadTimeoutMilliseconds { get; init; } = 500;

    public int StableWeightTimeoutSeconds { get; init; } = 5;
}