using PurchaseTicket.Infrastructure.Communication.Serial;
using PurchaseTicket.Infrastructure.Weighing;

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

        public FakeSerialPort(params string[] lines)
        {
            _lines = new Queue<string>(lines);
        }

        public bool IsOpen { get; private set; }

        public void Open()
        {
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

        var reader = new DiniArgeoWeightReader(
            serialPort,
            CreateOptions());

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

        var reader = new DiniArgeoWeightReader(
            serialPort,
            CreateOptions());

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

        var reader = new DiniArgeoWeightReader(
            serialPort,
            CreateOptions());

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

        var reader = new DiniArgeoWeightReader(
            serialPort,
            CreateOptions());

        // Act
        Task act() =>
            reader.ReadStableWeightAsync();

        // Assert
        await Assert.ThrowsAsync<IOException>(act);

        Assert.True(serialPort.WasOpened);
        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task ReadStableWeightAsync_ShouldConfigureReadTimeout()
    {
        // Arrange
        var serialPort = new FakeSerialPort(
            "ST,GS,    5980,kg\r");

        var options = new WeightIndicatorOptions
        {
            ReadTimeoutMilliseconds = 750,
            StableWeightTimeoutSeconds = 5
        };

        var reader = new DiniArgeoWeightReader(
            serialPort,
            options);

        // Act
        await reader.ReadStableWeightAsync();

        // Assert
        Assert.Equal(
            750,
            serialPort.ReadTimeout);
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

        var reader = new DiniArgeoWeightReader(
            serialPort,
            options);

        // Act
        Task act() =>
            reader.ReadStableWeightAsync();

        // Assert
        await Assert.ThrowsAsync<TimeoutException>(act);

        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
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

        var reader = new DiniArgeoWeightReader(
            serialPort,
            options);

        // Act
        var weight =
            await reader.ReadStableWeightAsync();

        // Assert
        Assert.Equal(5980m, weight);
    }
}