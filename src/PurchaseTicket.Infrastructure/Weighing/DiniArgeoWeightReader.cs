using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Weighing;
using PurchaseTicket.Infrastructure.Weighing.Serial;

namespace PurchaseTicket.Infrastructure.Weighing;

internal sealed class DiniArgeoWeightReader : IWeightReader
{
    private readonly IWeightIndicatorSerialPortFactory _serialPortFactory;
    private readonly WeightIndicatorOptions _options;
    private readonly ILogger<DiniArgeoWeightReader> _logger;

    public DiniArgeoWeightReader(
        IWeightIndicatorSerialPortFactory serialPortFactory,
        WeightIndicatorOptions options,
        ILogger<DiniArgeoWeightReader> logger)
    {
        _serialPortFactory = serialPortFactory;
        _options = options;
        _logger = logger;
    }

    public Task<decimal> ReadStableWeightAsync()
    {
        using var serialPort =
            _serialPortFactory.Create();

        try
        {
            serialPort.Open();
        }
        catch (Exception ex)
        {
            // El puerto no existe, esta ocupado o no hay permisos
            _logger.LogError(
                ex,
                "No se pudo abrir el puerto {PortName} del indicador de peso.",
                _options.PortName);

            throw;
        }

        try
        {
            serialPort.DiscardInBuffer();

            var stopwatch = Stopwatch.StartNew();

            // Datos para diagnosticar una lectura fallida
            int receivedLines = 0;
            string? lastLine = null;

            while (stopwatch.Elapsed <
                   TimeSpan.FromSeconds(
                       _options.StableWeightTimeoutSeconds))
            {
                try
                {
                    var data = serialPort.ReadLine();

                    receivedLines++;
                    lastLine = data;

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
                    // Seguir intentando hasta agotar el tiempo total
                }
            }

            // Cero lineas: el indicador no envia datos.
            // Con lineas: peso inestable o formato no reconocido.
            _logger.LogWarning(
                "No se obtuvo un peso estable del indicador en {TimeoutSeconds} s. " +
                "Líneas recibidas: {ReceivedLines}. Última línea: {LastLine}",
                _options.StableWeightTimeoutSeconds,
                receivedLines,
                lastLine?.Trim());

            throw new TimeoutException(
                $"No se pudo obtener un peso estable en " +
                $"{_options.StableWeightTimeoutSeconds} segundos.");
        }
        catch (IOException ex)
        {
            // Se perdio la comunicacion durante la lectura
            _logger.LogError(
                ex,
                "Error de comunicación con el indicador de peso en el puerto {PortName}.",
                _options.PortName);

            throw;
        }
        finally
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }
}