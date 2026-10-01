using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Infrastructure.Printing;
using PurchaseTicket.Infrastructure.Printing.Serial;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Infrastructure.Tests.Printing;

public class EpsonTmU295TicketPrinterTests
{
    private sealed class FakeSerialPortFactory
        : ISerialPortFactory
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

    private sealed class FakeSerialPort
        : ISerialPort
    {
        public bool IsOpen { get; private set; }

        public bool WasOpened { get; private set; }
        public bool WasWritten { get; private set; }
        public bool WasClosed { get; private set; }

        public string? WrittenText { get; private set; }

        public bool ThrowOnWrite { get; set; }

        public void Open()
        {
            IsOpen = true;
            WasOpened = true;
        }

        public void Write(string text)
        {
            if (ThrowOnWrite)
                throw new IOException(
                    "Serial write failed.");

            WasWritten = true;
            WrittenText = text;
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

    private static TicketPrinterOptions CreateOptions()
    {
        return new TicketPrinterOptions
        {
            CompanyName = "NOMBRE EMPRESA",
            Address = "DIRECCION CALLE",
            Neighborhood = "COLONIA",
            PhoneNumber = "55 0000 0000",
            Email = "correo@empresa.com"
        };
    }

    [Fact]
    public async Task PrintInitialAsync_ShouldOpenWriteAndCloseSerialPort()
    {
        // Arrange
        var serialPort = new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =new EpsonTmU295TicketPrinter(
            serialPortFactory,
            CreateOptions());

        var ticket = new Ticket(
            "1",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m);

        var printData = new TicketPrintData(
            ticket,
            "Proveedor Uno",
            "Acero");

        // Act
        await printer.PrintInitialAsync(printData);

        // Assert
        Assert.True(serialPort.WasOpened);
        Assert.True(serialPort.WasWritten);
        Assert.True(serialPort.WasClosed);
    }

    [Fact]
    public async Task PrintInitialAsync_ShouldWriteInitialTicketFormat()
    {
        // Arrange
        var serialPort = new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer = new EpsonTmU295TicketPrinter(
            serialPortFactory,
            CreateOptions());

        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC-123",
            "Juan Ortega",
            25000m);

        var printData = new TicketPrintData(
            ticket,
            "Proveedor Ejemplo",
            "Aluminio");

        // Act
        await printer.PrintInitialAsync(printData);

        // Assert
        var expected =
            "NOMBRE EMPRESA\r\n" +
            "DIRECCION CALLE\r\n" +
            "COLONIA\r\n" +
            "TEL. 55 0000 0000\r\n" +
            "correo@empresa.com\r\n" +
            "===================================\r\n" +
            "FOLIO    : T-000001\r\n" +
            "PLACAS   : ABC-123\r\n" +
            "PROVEEDOR: Proveedor Ejemplo\r\n" +
            "CHOFER   : Juan Ortega\r\n" +
            "PRODUCTO : Aluminio\r\n" +
            "-----------------------------------\r\n" +
            "ENTRADA\r\n" +
            $"FECHA    : {ticket.CreatedAt:dd/MM/yyyy HH:mm}\r\n" +
            "PESO     : 25,000 kg\r\n";

        Assert.Equal(
            expected,
            serialPort.WrittenText);
    }

    [Fact]
    public async Task PrintInitialAsync_ShouldCloseSerialPort_WhenWriteFails()
    {
        // Arrange
        var serialPort = new FakeSerialPort
        {
            ThrowOnWrite = true
        };

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer = new EpsonTmU295TicketPrinter(
            serialPortFactory,
            CreateOptions());

        var ticket = new Ticket(
            "123",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m);

        var printData = new TicketPrintData(
            ticket,
            "Proveedor Uno",
            "Acero");

        // Act
        Task act() =>
            printer.PrintInitialAsync(printData);

        // Assert
        await Assert.ThrowsAsync<IOException>(act);

        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task PrintInitialAsync_ShouldNotExceed35CharactersPerLine()
    {
        // Arrange
        var serialPort = new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer = new EpsonTmU295TicketPrinter(
            serialPortFactory,
            CreateOptions());

        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC-123",
            "Juan Ortega",
            25000m);

        var printData = new TicketPrintData(
            ticket,
            "Comercializadora de Metales del Centro",
            "Aluminio");

        // Act
        await printer.PrintInitialAsync(printData);

        // Assert
        Assert.NotNull(serialPort.WrittenText);

        var lines = serialPort.WrittenText.Split(
            "\r\n",
            StringSplitOptions.RemoveEmptyEntries);

        Assert.All(
            lines,
            line => Assert.True(
                line.Length <= 35,
                $"Line exceeds 35 characters: '{line}'"));
    }

    [Fact]
    public async Task PrintInitialAsync_ShouldWrapLongSupplierName()
    {
        // Arrange
        var serialPort = new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer = new EpsonTmU295TicketPrinter(
            serialPortFactory,
            CreateOptions());

        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC-123",
            "Juan Ortega",
            25000m);

        var printData = new TicketPrintData(
            ticket,
            "Comercializadora de Metales del Centro",
            "Aluminio");

        // Act
        await printer.PrintInitialAsync(printData);

        // Assert
        Assert.NotNull(serialPort.WrittenText);

        Assert.Contains(
            "PROVEEDOR: Comercializadora de\r\n" +
            "           Metales del Centro\r\n",
            serialPort.WrittenText);
    }

    [Fact]
    public async Task PrintFinalAsync_ShouldWriteFinalTicketFormat()
    {
        // Arrange
        var serialPort = new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer = new EpsonTmU295TicketPrinter(
            serialPortFactory,
            CreateOptions());

        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC-123",
            "Juan Ortega",
            25000m);

        ticket.Complete(
            tareWeight: 10000m,
            discount: 0m,
            pricePerKg: 1m);

        var printData = new TicketPrintData(
            ticket,
            "Proveedor Ejemplo",
            "Aluminio");

        // Act
        await printer.PrintFinalAsync(printData);

        // Assert
        var expected =
            "-----------------------------------\r\n" +
            "SALIDA\r\n" +
            $"FECHA    : {ticket.CompletedAt:dd/MM/yyyy HH:mm}\r\n" +
            "PESO     : 10,000 kg\r\n" +
            "-----------------------------------\r\n" +
            "PESO NETO: 15,000 kg\r\n" +
            "===================================\r\n";

        Assert.Equal(
            expected,
            serialPort.WrittenText);
    }

    [Fact]
    public async Task PrintFinalAsync_ShouldCloseSerialPort_WhenWriteFails()
    {
        // Arrange
        var serialPort = new FakeSerialPort
        {
            ThrowOnWrite = true
        };

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer = new EpsonTmU295TicketPrinter(
            serialPortFactory,
            CreateOptions());

        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC-123",
            "Juan Ortega",
            25000m);

        ticket.Complete(
            tareWeight: 10000m,
            discount: 0m,
            pricePerKg: 1m);

        var printData = new TicketPrintData(
            ticket,
            "Proveedor Ejemplo",
            "Aluminio");

        // Act
        Task act() =>
            printer.PrintFinalAsync(printData);

        // Assert
        await Assert.ThrowsAsync<IOException>(act);

        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task PrintFinalAsync_ShouldNotExceed35CharactersPerLine()
    {
        // Arrange
        var serialPort = new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer = new EpsonTmU295TicketPrinter(
            serialPortFactory,
            CreateOptions());

        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC-123",
            "Juan Ortega",
            25000m);

        ticket.Complete(
            tareWeight: 10000m,
            discount: 0m,
            pricePerKg: 1m);

        var printData = new TicketPrintData(
            ticket,
            "Proveedor Ejemplo",
            "Aluminio");

        // Act
        await printer.PrintFinalAsync(printData);

        // Assert
        Assert.NotNull(serialPort.WrittenText);

        var lines = serialPort.WrittenText.Split(
            "\r\n",
            StringSplitOptions.RemoveEmptyEntries);

        Assert.All(
            lines,
            line => Assert.True(
                line.Length <= 35,
                $"Line exceeds 35 characters: '{line}'"));
    }
}