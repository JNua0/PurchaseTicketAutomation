using System.Diagnostics;
using PurchaseTicket.Application.Abstractions.Weighing;
using PurchaseTicket.Infrastructure.Weighing.Serial;

namespace PurchaseTicket.Infrastructure.Weighing;

internal sealed class DiniArgeoWeightReader : IWeightReader
{
    private readonly IWeightIndicatorSerialPortFactory
        _serialPortFactory;

    private readonly WeightIndicatorOptions _options;

    public DiniArgeoWeightReader(
        IWeightIndicatorSerialPortFactory serialPortFactory,
        WeightIndicatorOptions options)
    {
        _serialPortFactory = serialPortFactory;
        _options = options;
    }

    public Task<decimal> ReadStableWeightAsync()
    {
        using var serialPort =
            _serialPortFactory.Create();

        serialPort.Open();

        try
        {
            serialPort.DiscardInBuffer();

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed <
                   TimeSpan.FromSeconds(
                       _options.StableWeightTimeoutSeconds))
            {
                try
                {
                    var data = serialPort.ReadLine();

                    if (DiniArgeoWeightParser
                        .TryParseStableWeight(
                            data,
                            out var weight))
                    {
                        return Task.FromResult(weight);
                    }
                }
                catch (TimeoutException)
                {
                    // Continue trying until the global
                    // stable-weight timeout expires.
                }
            }

            throw new TimeoutException(
                $"No se pudo obtener un peso estable en " +
                $"{_options.StableWeightTimeoutSeconds} segundos.");
        }
        finally
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }
}