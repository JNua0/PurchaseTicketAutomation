using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Reprint;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class ReprintPurchaseTicketTests
{
    private sealed class FakePurchaseTicketRepository
        : IPurchaseTicketRepository
    {
        public Ticket? Ticket { get; init; }
        public Ticket? UpdatedTicket { get; private set; }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            return Task.FromResult(Ticket);
        }

        public Task UpdateAsync(Ticket purchaseTicket)
        {
            UpdatedTicket = purchaseTicket;
            return Task.CompletedTask;
        }

        public Task AddAsync(Ticket purchaseTicket)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Ticket>> GetPendingAsync()
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Ticket>> GetAllAsync()
            => throw new NotImplementedException();
    }

    private sealed class FakeTicketPrinter : ITicketPrinter
    {
        public Ticket? PrintedTicket { get; private set; }
        public bool ThrowOnPrint { get; init; }

        public Task PrintInitialAsync(Ticket purchaseTicket)
        {
            throw new NotImplementedException();
        }

        public Task PrintFinalAsync(Ticket purchaseTicket)
        {
            if (ThrowOnPrint)
                throw new InvalidOperationException(
                    "Printer error.");

            PrintedTicket = purchaseTicket;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReprintCompletedPurchaseTicket()
    {
        // Arrange
        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierCustomerId: 1,
            materialId: 2,
            licensePlate: "ABC-123",
            driverName: "Juan Perez",
            grossWeight: 25000m);

        ticket.Complete(
            tareWeight: 10000m,
            discount: 5m,
            pricePerKg: 2m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var printer = new FakeTicketPrinter();

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new ReprintPurchaseTicket(
            repository,
            printer);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Same(ticket, printer.PrintedTicket);
        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketDoesNotExist()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository
        {
            Ticket = null
        };

        var printer = new FakeTicketPrinter();

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 999);

        var useCase = new ReprintPurchaseTicket(
            repository,
            printer);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "Purchase ticket was not found.",
            exception.Message);

        Assert.Null(printer.PrintedTicket);
        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketIsNotCompleted()
    {
        // Arrange
        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierCustomerId: 1,
            materialId: 2,
            licensePlate: "ABC-123",
            driverName: "Juan Perez",
            grossWeight: 25000m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var printer = new FakeTicketPrinter();

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new ReprintPurchaseTicket(
            repository,
            printer);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "Only completed purchase tickets can be reprinted.",
            exception.Message);

        Assert.Null(printer.PrintedTicket);
        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketIsCancelled()
    {
        // Arrange
        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierCustomerId: 1,
            materialId: 2,
            licensePlate: "ABC-123",
            driverName: "Juan Perez",
            grossWeight: 25000m);

        ticket.Cancel();

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var printer = new FakeTicketPrinter();

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new ReprintPurchaseTicket(
            repository,
            printer);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "Only completed purchase tickets can be reprinted.",
            exception.Message);

        Assert.Null(printer.PrintedTicket);
        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPrintedFalseWhenPrinterFails()
    {
        // Arrange
        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierCustomerId: 1,
            materialId: 2,
            licensePlate: "ABC-123",
            driverName: "Juan Perez",
            grossWeight: 25000m);

        ticket.Complete(
            tareWeight: 10000m,
            discount: 5m,
            pricePerKg: 2m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var printer = new FakeTicketPrinter
        {
            ThrowOnPrint = true
        };

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new ReprintPurchaseTicket(
            repository,
            printer);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal("T-000001", result.TicketNumber);
        Assert.False(result.Printed);
        Assert.Null(repository.UpdatedTicket);
    }
}