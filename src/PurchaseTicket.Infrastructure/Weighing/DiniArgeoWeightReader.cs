using System.Diagnostics;
using PurchaseTicket.Application.Abstractions.Weighing;
using PurchaseTicket.Infrastructure.Communication.Serial;

namespace PurchaseTicket.Infrastructure.Weighing;

internal sealed class DiniArgeoWeightReader : IWeightReader
{
    private readonly ISerialPort _serialPort;
    private readonly WeightIndicatorOptions _options;

    public DiniArgeoWeightReader(
        ISerialPort serialPort,
        WeightIndicatorOptions options)
    {
        _serialPort = serialPort;
        _options = options;
    }

    public Task<decimal> ReadStableWeightAsync()
    {
        _serialPort.ReadTimeout =
            _options.ReadTimeoutMilliseconds;

        _serialPort.Open();

        try
        {
            _serialPort.DiscardInBuffer();

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed <
                   TimeSpan.FromSeconds(
                       _options.StableWeightTimeoutSeconds))
            {
                try
                {
                    var data = _serialPort.ReadLine();

                    if (DiniArgeoWeightParser.TryParseStableWeight(
                            data,
                            out var weight))
                    {
                        return Task.FromResult(weight);
                    }
                }
                catch (TimeoutException)
                {
                    // No se recibió una trama completa durante
                    // ReadTimeoutMilliseconds.
                    // Continuamos mientras no se alcance
                    // StableWeightTimeoutSeconds.
                }
            }

            throw new TimeoutException(
                $"No se pudo obtener un peso estable en " +
                $"{_options.StableWeightTimeoutSeconds} segundos.");
        }
        finally
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }
    }
}