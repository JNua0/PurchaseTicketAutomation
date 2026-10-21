using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.UseCases.Printing.PrintTest;

namespace PurchaseTicket.Application.Tests.UseCases.Printing;

public class PrintTestTicketTests
{
    private sealed class FakeTicketPrinter : ITicketPrinter
    {
        public Exception? Failure { get; init; }

        public bool WasTestPrinted { get; private set; }

        public Task PrintTestAsync()
        {
            if (Failure is not null)
                throw Failure;

            WasTestPrinted = true;

            return Task.CompletedTask;
        }

        public Task PrintConventionalInitialAsync(TicketPrintData data)
            => throw new NotImplementedException();

        public Task PrintConventionalFinalAsync(TicketPrintData data)
            => throw new NotImplementedException();

        public Task PrintConventionalCompletedAsync(TicketPrintData data)
            => throw new NotImplementedException();

        public Task PrintSingleAsync(TicketPrintData data)
            => throw new NotImplementedException();
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, Exception? Exception)> Entries { get; } = new();

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
            Entries.Add((logLevel, exception));
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNone_WhenTestTicketIsPrinted()
    {
        // Arrange
        var printer = new FakeTicketPrinter();
        var logger = new FakeLogger<PrintTestTicket>();

        var useCase = new PrintTestTicket(printer, logger);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Equal(PrinterErrorCode.None, result);
        Assert.True(printer.WasTestPrinted);
        Assert.Empty(logger.Entries);
    }

    [Theory]
    [InlineData("puerto inexistente", PrinterErrorCode.PortNotFound)]
    [InlineData("acceso denegado", PrinterErrorCode.PortAccessDenied)]
    [InlineData("comunicacion", PrinterErrorCode.CommunicationError)]
    [InlineData("desconocida", PrinterErrorCode.Unknown)]
    public async Task ExecuteAsync_ShouldReturnErrorCodeAndLog_WhenPrintingFails(
        string failure,
        PrinterErrorCode expectedErrorCode)
    {
        // Arrange
        Exception exception = failure switch
        {
            "puerto inexistente" => new FileNotFoundException(),
            "acceso denegado" => new UnauthorizedAccessException(),
            "comunicacion" => new IOException(),
            _ => new TimeoutException()
        };

        var printer = new FakeTicketPrinter { Failure = exception };
        var logger = new FakeLogger<PrintTestTicket>();

        var useCase = new PrintTestTicket(printer, logger);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Equal(expectedErrorCode, result);

        var entry = Assert.Single(logger.Entries);

        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Same(exception, entry.Exception);
    }
}