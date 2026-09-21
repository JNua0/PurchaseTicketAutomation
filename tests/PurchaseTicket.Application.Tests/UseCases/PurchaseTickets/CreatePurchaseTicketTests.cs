using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Create;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;
using PurchaseTicket.Application.Abstractions.Printing;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CreatePurchaseTicketTests
{
    private sealed class FakePurchaseTicketRepository
        : IPurchaseTicketRepository
    {
        public Ticket? AddedTicket { get; private set; }

        public Task AddAsync(Ticket purchaseTicket)
        {
            AddedTicket = purchaseTicket;

            return Task.CompletedTask;
        }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Ticket purchaseTicket)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Ticket>> GetPendingAsync()
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeTicketNumberGenerator
        : ITicketNumberGenerator
    {
        public Task<string> GenerateAsync()
        {
            return Task.FromResult("T-000001");
        }
    }

    private sealed class FakeTicketPrinter : ITicketPrinter
    {
        private readonly bool _shouldFail;

        public Ticket? PrintedTicket { get; private set; }

        public FakeTicketPrinter(bool shouldFail = false)
        {
            _shouldFail = shouldFail;
        }

        public Task PrintInitialAsync(Ticket purchaseTicket)
        {
            if (_shouldFail)
                throw new InvalidOperationException(
                    "Printer unavailable.");

            PrintedTicket = purchaseTicket;

            return Task.CompletedTask;
        }

        public Task PrintFinalAsync(Ticket purchaseTicket)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateAndStorePendingPurchaseTicket()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketNumberGenerator = new FakeTicketNumberGenerator();
        var ticketPrinter = new FakeTicketPrinter();

        var useCase = new CreatePurchaseTicket(
            repository,
            ticketNumberGenerator,
            ticketPrinter);

        var command = new CreatePurchaseTicketCommand(
            SupplierCustomerId: 1,
            MaterialId: 2,
            LicensePlate: "abc-123",
            DriverName: "juan perez",
            GrossWeight: 25000m);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.AddedTicket);

        Assert.Equal(
            "T-000001",
            repository.AddedTicket.TicketNumber);

        Assert.Equal(
            1,
            repository.AddedTicket.SupplierCustomerId);

        Assert.Equal(
            2,
            repository.AddedTicket.MaterialId);

        Assert.Equal(
            25000m,
            repository.AddedTicket.GrossWeight);

        Assert.Equal(
            TicketStatus.Pending,
            repository.AddedTicket.Status);

        Assert.Equal(
            "ABC-123",
            repository.AddedTicket.LicensePlate);

        Assert.Equal(
            "Juan Perez",
            repository.AddedTicket.DriverName);

        Assert.NotNull(ticketPrinter.PrintedTicket);

        Assert.Same(
            repository.AddedTicket,
            ticketPrinter.PrintedTicket);

        Assert.True(result.Printed);
        Assert.Equal("T-000001", result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotStoreTicketWhenDataIsInvalid()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketNumberGenerator = new FakeTicketNumberGenerator();
        var ticketPrinter = new FakeTicketPrinter();

        var useCase = new CreatePurchaseTicket(
            repository,
            ticketNumberGenerator,
            ticketPrinter);

        var command = new CreatePurchaseTicketCommand(
            SupplierCustomerId: 1,
            MaterialId: 2,
            LicensePlate: "abc-123",
            DriverName: "juan perez",
            GrossWeight: 0m);

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Null(repository.AddedTicket);
        Assert.Null(ticketPrinter.PrintedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepStoredTicketWhenPrintingFails()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketNumberGenerator = new FakeTicketNumberGenerator();
        var ticketPrinter = new FakeTicketPrinter(
            shouldFail: true);

        var useCase = new CreatePurchaseTicket(
            repository,
            ticketNumberGenerator,
            ticketPrinter);

        var command = new CreatePurchaseTicketCommand(
            SupplierCustomerId: 1,
            MaterialId: 2,
            LicensePlate: "abc-123",
            DriverName: "juan perez",
            GrossWeight: 25000m);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.AddedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.False(result.Printed);
    }
}