using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PurchaseTicket.Infrastructure.Communication.Serial;
using PurchaseTicket.Infrastructure.Weighing;
using PurchaseTicket.Infrastructure.Weighing.Serial;

namespace PurchaseTicket.Infrastructure.Tests.Weighing;

public class DiniArgeoWeightReaderTests
{
    private sealed class FakeSerialPort : ISerialPort
    {
        private readonly Queue<string> _lines;
        public bool WasOpened { get; private set; }
        public bool WasBufferDiscarded { get; private set; }
        public bool WasClosed { get; private set; }
        public bool ThrowOnRead { get; set; }
        public int ReadTimeout { get; set; }
        public int ReadTimeoutsBeforeData { get; set; }
        public bool ThrowOnOpen { get; set; }

        public FakeSerialPort(params string[] lines)
        {
            _lines = new Queue<string>(lines);
        }

        public bool IsOpen { get; private set; }

        public void Open()
        {
            if (ThrowOnOpen)
                throw new FileNotFoundException(
                    "Could not find file 'COM9'.");

            IsOpen = true;
            WasOpened = true;
        }

        public string ReadLine()
        {
            if (ThrowOnRead)
                throw new IOException(
                    "Serial read failed.");

            if (ReadTimeoutsBeforeData > 0)
            {
                ReadTimeoutsBeforeData--;

                throw new TimeoutException();
            }

            if (_lines.Count == 0)
                throw new TimeoutException();

            return _lines.Dequeue();
        }

        public void DiscardInBuffer()
        {
            WasBufferDiscarded = true;
        }

        public void Write(string text)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            IsOpen = false;
            WasClosed = true;
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeSerialPortFactory
        : IWeightIndicatorSerialPortFactory
    {
        private readonly ISerialPort _serialPort;

        public FakeSerialPortFactory(
            ISerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public ISerialPort Create()
        {
            return _serialPort;
        }
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        // Cada mensaje registrado con su nivel y excepcion
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // El formatter arma el texto final con los valores de la plantilla
            Entries.Add((logLevel, formatter(state, exception), exception));
        }
    }

    private static WeightIndicatorOptions CreateOptions()
    {
        return new WeightIndicatorOptions
        {
            ReadTimeoutMilliseconds = 500,
            StableWeightTimeoutSeconds = 5
        };
    }

    [Fact]
    public async Task ReadStableWeightAsync_ShouldReturnStableWeight()
    {
        // Arrange
        var serialPort = new FakeSerialPort(
            "ST,GS,25000,kg");

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var reader = new DiniArgeoWeightReader(
            serialPortFactory,
            CreateOptions(),
            NullLogger<DiniArgeoWeightReader>.Instance);

        // Act
        var weight =
            await reader.ReadStableWeightAsync();

        // Assert
        Assert.Equal(25000m, weight);
    }

    [Fact]
    public async Task ReadStableWeightAsync_ShouldIgnoreUnstableWeights()
    {
        // Arrange
        var serialPort = new FakeSerialPort(
            "US,GS,    5970,kg\r",
            "US,GS,    5985,kg\r",
            "ST,GS,    5980,kg\r");

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var reader = new DiniArgeoWeightReader(
            serialPortFactory,
            CreateOptions(),
            NullLogger<DiniArgeoWeightReader>.Instance);

        // Act
        var weight =
            await reader.ReadStableWeightAsync();

        // Assert
        Assert.Equal(5980m, weight);
    }

    [Fact]
    public async Task ReadStableWeightAsync_ShouldOpenDiscardBufferAndCloseSerialPort()
    {
        // Arrange
        var serialPort = new FakeSerialPort(
            "ST,GS,    5980,kg\r");

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var reader = new DiniArgeoWeightReader(
            serialPortFactory,
            CreateOptions(),
            NullLogger<DiniArgeoWeightReader>.Instance);

        // Act
        await reader.ReadStableWeightAsync();

        // Assert
        Assert.True(serialPort.WasOpened);
        Assert.True(serialPort.WasBufferDiscarded);
        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task ReadStableWeightAsync_ShouldCloseSerialPort_WhenReadFails()
    {
        // Arrange
        var serialPort = new FakeSerialPort
        {
            ThrowOnRead = true
        };

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var logger =
            new FakeLogger<DiniArgeoWeightReader>();

        var reader = new DiniArgeoWeightReader(
            serialPortFactory,
            CreateOptions(),
            logger);

        // Act
        Task act() =>
            reader.ReadStableWeightAsync();

        // Assert
        await Assert.ThrowsAsync<IOException>(act);

        Assert.True(serialPort.WasOpened);
        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);

        // Debe registrar el error de comunicacion con la excepcion original
        var entry = Assert.Single(logger.Entries);

        Assert.Equal(LogLevel.Error, entry.Level);

        Assert.IsType<IOException>(entry.Exception);
    }

    [Fact]
    public async Task ReadStableWeightAsync_ShouldThrowTimeout_WhenStableWeightIsNotReceived()
    {
        // Arrange
        var serialPort = new FakeSerialPort(
            "US,GS,    5970,kg\r",
            "US,GS,    5980,kg\r",
            "US,GS,    5990,kg\r");

        var options = new WeightIndicatorOptions
        {
            ReadTimeoutMilliseconds = 50,
            StableWeightTimeoutSeconds = 1
        };

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var logger =
            new FakeLogger<DiniArgeoWeightReader>();

        var reader = new DiniArgeoWeightReader(
            serialPortFactory,
            options,
            logger);

        // Act
        Task act() =>
            reader.ReadStableWeightAsync();

        // Assert
        await Assert.ThrowsAsync<TimeoutException>(act);

        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);

        // Debe dejar el diagnostico: 3 lineas inestables recibidas
        var entry = Assert.Single(logger.Entries);

        Assert.Equal(LogLevel.Warning, entry.Level);

        Assert.Contains("Líneas recibidas: 3", entry.Message);

        Assert.Contains("Última línea: US,GS,    5990,kg", entry.Message);
    }

    [Fact]
    public async Task ReadStableWeightAsync_ShouldContinueReading_AfterReadTimeout()
    {
        // Arrange
        var serialPort = new FakeSerialPort(
            "ST,GS,    5980,kg\r")
        {
            ReadTimeoutsBeforeData = 2
        };

        var options = new WeightIndicatorOptions
        {
            ReadTimeoutMilliseconds = 50,
            StableWeightTimeoutSeconds = 1
        };

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var reader = new DiniArgeoWeightReader(
            serialPortFactory,
            CreateOptions(),
            NullLogger<DiniArgeoWeightReader>.Instance);

        // Act
        var weight =
            await reader.ReadStableWeightAsync();

        // Assert
        Assert.Equal(5980m, weight);
    }

    [Fact]
    public async Task ReadStableWeightAsync_ShouldLogPortName_WhenPortCannotBeOpened()
    {
        // Arrange
        var serialPort = new FakeSerialPort
        {
            ThrowOnOpen = true
        };

        var options = new WeightIndicatorOptions
        {
            PortName = "COM9"
        };

        var logger =
            new FakeLogger<DiniArgeoWeightReader>();

        var reader = new DiniArgeoWeightReader(
            new FakeSerialPortFactory(serialPort),
            options,
            logger);

        // Act
        Task act() =>
            reader.ReadStableWeightAsync();

        // Assert
        await Assert.ThrowsAsync<FileNotFoundException>(act);

        var entry = Assert.Single(logger.Entries);

        Assert.Equal(LogLevel.Error, entry.Level);

        Assert.Equal(
            "No se pudo abrir el puerto COM9 del indicador de peso.",
            entry.Message);
    }
}