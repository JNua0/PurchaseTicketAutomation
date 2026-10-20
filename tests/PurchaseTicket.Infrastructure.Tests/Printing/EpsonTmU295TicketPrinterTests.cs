using System.Globalization;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Infrastructure.Printing;
using PurchaseTicket.Infrastructure.Printing.Serial;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;
using PurchaseTicket.Infrastructure.Communication.Serial;

namespace PurchaseTicket.Infrastructure.Tests.Printing;

public class EpsonTmU295TicketPrinterTests
{
    // Zona horaria fija UTC-06:00 para que las pruebas no dependan del equipo
    private static readonly TimeZoneInfo TestTimeZone =
        TimeZoneInfo.CreateCustomTimeZone(
            "UTC-06",
            TimeSpan.FromHours(-6),
            "UTC-06",
            "UTC-06");

    private static readonly TimeProvider TestTimeProvider =
        new FixedTimeZoneProvider(TestTimeZone);

    private sealed class FixedTimeZoneProvider : TimeProvider
    {
        private readonly TimeZoneInfo _timeZone;

        public FixedTimeZoneProvider(TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
        }

        public override TimeZoneInfo LocalTimeZone => _timeZone;
    }

    // Fecha esperada en el ticket: UTC menos 6 horas
    private static string LocalDate(DateTime utcDate)
    {
        return utcDate
            .AddHours(-6)
            .ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
    }

    private sealed class FakeSerialPortFactory
        : ISerialPortFactory
    {
        private readonly ISerialPort _serialPort;

        public FakeSerialPortFactory(ISerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public ISerialPort Create()
        {
            return _serialPort;
        }
    }

    private sealed class FakeSerialPort : ISerialPort
    {
        public bool IsOpen { get; private set; }
        public bool WasOpened { get; private set; }
        public bool WasWritten { get; private set; }
        public bool WasClosed { get; private set; }
        public string? WrittenText { get; private set; }
        public bool ThrowOnWrite { get; set; }
        public int ReadTimeout { get; set; }

        public void Open()
        {
            IsOpen = true;
            WasOpened = true;
        }

        public void Write(string text)
        {
            if (ThrowOnWrite)
                throw new IOException("Serial write failed.");

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

        public string ReadLine()
        {
            throw new NotImplementedException();
        }

        public void DiscardInBuffer()
        {
            throw new NotImplementedException();
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

    private static Ticket CreateInitialTicket(
        string ticketNumber = "T-000001",
        string licensePlate = "ABC123",
        string transporter = "Juan Ortega",
        decimal grossWeight = 25000m)
    {
        return Ticket.CreateConventional(
            ticketNumber,
            1,
            1,
            licensePlate,
            transporter,
            grossWeight);
    }

    private static Ticket CreateTicketWithTare()
    {
        var ticket = CreateInitialTicket();

        ticket.RegisterTare(10000m);

        return ticket;
    }

    private static Ticket CreateCompletedConventionalTicket()
    {
        var ticket = CreateTicketWithTare();

        ticket.RegisterAmount(
            discount: 5,
            pricePerKg: 3m);

        return ticket;
    }

    private static Ticket CreateSingleTicket()
    {
        return Ticket.CreateSingle(
            ticketNumber: "T-000002",
            supplierId: 1,
            materialId: 1,
            licensePlate: "XYZ789",
            transporter: "Pedro Lopez",
            netWeight: 12500m);
    }

    [Fact]
    public async Task PrintConventionalInitialAsync_ShouldOpenWriteAndCloseSerialPort()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateInitialTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Uno",
                "Acero");

        await printer.PrintConventionalInitialAsync(
            printData);

        Assert.True(serialPort.WasOpened);
        Assert.True(serialPort.WasWritten);
        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task PrintConventionalInitialAsync_ShouldWriteInitialTicketFormat()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateInitialTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Ejemplo",
                "Aluminio");

        await printer.PrintConventionalInitialAsync(
            printData);

        var expected =
            "NOMBRE EMPRESA\r\n" +
            "DIRECCION CALLE\r\n" +
            "COLONIA\r\n" +
            "TEL. 55 0000 0000\r\n" +
            "correo@empresa.com\r\n" +
            "===================================\r\n" +
            "FOLIO    : T-000001\r\n" +
            "PLACAS   : ABC123\r\n" +
            "PROVEEDOR: Proveedor Ejemplo\r\n" +
            "CHOFER   : Juan Ortega\r\n" +
            "PRODUCTO : Aluminio\r\n" +
            "-----------------------------------\r\n" +
            "ENTRADA\r\n" +
            $"FECHA    : {LocalDate(ticket.CheckInAt)}\r\n" +
            "PESO     : 25,000 kg\r\n";

        Assert.Equal(
            expected,
            serialPort.WrittenText);
    }

    [Fact]
    public async Task PrintConventionalInitialAsync_ShouldCloseSerialPort_WhenWriteFails()
    {
        var serialPort =
            new FakeSerialPort
            {
                ThrowOnWrite = true
            };

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateInitialTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Uno",
                "Acero");

        Task act() =>
            printer.PrintConventionalInitialAsync(
                printData);

        await Assert.ThrowsAsync<IOException>(act);

        Assert.True(serialPort.WasOpened);
        Assert.False(serialPort.WasWritten);
        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task PrintConventionalInitialAsync_ShouldNotExceed35CharactersPerLine()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateInitialTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Comercializadora de Metales del Centro",
                "Aluminio");

        await printer.PrintConventionalInitialAsync(
            printData);

        Assert.NotNull(serialPort.WrittenText);

        var lines =
            serialPort.WrittenText.Split(
                "\r\n",
                StringSplitOptions.RemoveEmptyEntries);

        Assert.All(
            lines,
            line => Assert.True(
                line.Length <= 35,
                $"Line exceeds 35 characters: '{line}'"));
    }

    [Fact]
    public async Task PrintConventionalInitialAsync_ShouldWrapLongSupplierName()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateInitialTicket();

        const string supplierName =
            "Comercializadora de Metales del Centro";

        var printData =
            new TicketPrintData(
                ticket,
                supplierName,
                "Aluminio");

        await printer.PrintConventionalInitialAsync(
            printData);

        Assert.NotNull(serialPort.WrittenText);

        Assert.Contains(
            "PROVEEDOR: Comercializadora de",
            serialPort.WrittenText);

        Assert.Contains(
            "           Metales del Centro",
            serialPort.WrittenText);
    }

    [Fact]
    public async Task PrintConventionalFinalAsync_ShouldWriteFinalTicketFormat()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateTicketWithTare();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Uno",
                "Acero");

        await printer.PrintConventionalFinalAsync(
            printData);

        var expected =
            "-----------------------------------\r\n" +
            "SALIDA\r\n" +
            $"FECHA    : {LocalDate(ticket.DepartureAt!.Value)}\r\n" +
            "PESO     : 10,000 kg\r\n" +
            "-----------------------------------\r\n" +
            "PESO NETO: 15,000 kg\r\n" +
            "===================================\r\n";

        Assert.Equal(
            expected,
            serialPort.WrittenText);
    }

    [Fact]
    public async Task PrintConventionalFinalAsync_ShouldCloseSerialPort_WhenWriteFails()
    {
        var serialPort =
            new FakeSerialPort
            {
                ThrowOnWrite = true
            };

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateTicketWithTare();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Uno",
                "Acero");

        Task act() =>
            printer.PrintConventionalFinalAsync(
                printData);

        await Assert.ThrowsAsync<IOException>(act);

        Assert.True(serialPort.WasOpened);
        Assert.False(serialPort.WasWritten);
        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task PrintConventionalCompletedAsync_ShouldOpenWriteAndCloseSerialPort()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateCompletedConventionalTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Uno",
                "Acero");

        await printer.PrintConventionalCompletedAsync(
            printData);

        Assert.True(serialPort.WasOpened);
        Assert.True(serialPort.WasWritten);
        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task PrintConventionalCompletedAsync_ShouldWriteCompletedTicketFormat()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateCompletedConventionalTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Ejemplo",
                "Aluminio");

        await printer.PrintConventionalCompletedAsync(
            printData);

        var expected =
            "NOMBRE EMPRESA\r\n" +
            "DIRECCION CALLE\r\n" +
            "COLONIA\r\n" +
            "TEL. 55 0000 0000\r\n" +
            "correo@empresa.com\r\n" +
            "===================================\r\n" +
            "FOLIO    : T-000001\r\n" +
            "PLACAS   : ABC123\r\n" +
            "PROVEEDOR: Proveedor Ejemplo\r\n" +
            "CHOFER   : Juan Ortega\r\n" +
            "PRODUCTO : Aluminio\r\n" +
            "-----------------------------------\r\n" +
            "ENTRADA\r\n" +
            $"FECHA    : {LocalDate(ticket.CheckInAt)}\r\n" +
            "PESO     : 25,000 kg\r\n" +
            "-----------------------------------\r\n" +
            "SALIDA\r\n" +
            $"FECHA    : {LocalDate(ticket.DepartureAt!.Value)}\r\n" +
            "PESO     : 10,000 kg\r\n" +
            "-----------------------------------\r\n" +
            "PESO NETO: 15,000 kg\r\n" +
            "===================================\r\n";

        Assert.Equal(
            expected,
            serialPort.WrittenText);
    }

    [Fact]
    public async Task PrintSingleAsync_ShouldOpenWriteAndCloseSerialPort()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateSingleTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Uno",
                "Acero");

        await printer.PrintSingleAsync(
            printData);

        Assert.True(serialPort.WasOpened);
        Assert.True(serialPort.WasWritten);
        Assert.True(serialPort.WasClosed);
        Assert.False(serialPort.IsOpen);
    }

    [Fact]
    public async Task PrintSingleAsync_ShouldWriteSingleTicketFormat()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateSingleTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Uno",
                "Acero");

        await printer.PrintSingleAsync(
            printData);

        var expected =
            "NOMBRE EMPRESA\r\n" +
            "DIRECCION CALLE\r\n" +
            "COLONIA\r\n" +
            "TEL. 55 0000 0000\r\n" +
            "correo@empresa.com\r\n" +
            "===================================\r\n" +
            "FOLIO    : T-000002\r\n" +
            "PLACAS   : XYZ789\r\n" +
            "PROVEEDOR: Proveedor Uno\r\n" +
            "CHOFER   : Pedro Lopez\r\n" +
            "PRODUCTO : Acero\r\n" +
            "-----------------------------------\r\n" +
            "PESAJE UNICO\r\n" +
            $"FECHA    : {LocalDate(ticket.CheckInAt)}\r\n" +
            "PESO NETO: 12,500 kg\r\n";

        Assert.Equal(
            expected,
            serialPort.WrittenText);
    }

    [Fact]
    public async Task PrintSingleAsync_ShouldPrintDateInLocalTime()
    {
        var serialPort =
            new FakeSerialPort();

        var serialPortFactory =
            new FakeSerialPortFactory(serialPort);

        var printer =
            new EpsonTmU295TicketPrinter(
                serialPortFactory,
                CreateOptions(),
                TestTimeProvider);

        var ticket =
            CreateSingleTicket();

        var printData =
            new TicketPrintData(
                ticket,
                "Proveedor Uno",
                "Acero");

        await printer.PrintSingleAsync(
            printData);

        string utcDate =
            ticket.CheckInAt.ToString(
                "dd/MM/yyyy HH:mm",
                CultureInfo.InvariantCulture);

        Assert.NotNull(serialPort.WrittenText);

        // Debe imprimir la hora local y no la hora UTC guardada
        Assert.Contains(
            $"FECHA    : {LocalDate(ticket.CheckInAt)}",
            serialPort.WrittenText);

        Assert.DoesNotContain(
            $"FECHA    : {utcDate}",
            serialPort.WrittenText);
    }
}