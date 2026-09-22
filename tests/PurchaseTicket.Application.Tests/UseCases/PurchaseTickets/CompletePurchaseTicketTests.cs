using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Complete;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CompletePurchaseTicketTests
{
    private sealed class FakePurchaseTicketRepository
        : IPurchaseTicketRepository
    {
        public Ticket? Ticket { get; set; }
        public Ticket? UpdatedTicket { get; private set; }

        public Task AddAsync(Ticket purchaseTicket)
        {
            throw new NotImplementedException();
        }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            return Task.FromResult(Ticket);
        }

        public Task<IReadOnlyList<Ticket>> GetPendingAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Ticket purchaseTicket)
        {
            UpdatedTicket = purchaseTicket;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Ticket>> GetAllAsync()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Task PrintFinalAsync(Ticket purchaseTicket)
        {
            if (_shouldFail)
                throw new InvalidOperationException(
                    "Printer unavailable.");

            PrintedTicket = purchaseTicket;

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCompletePendingPurchaseTicket()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketPrinter = new FakeTicketPrinter();

        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "abc-123",
            driverName: "juan perez",
            grossWeight: 25000m);

        repository.Ticket = ticket;

        var useCase = new CompletePurchaseTicket(
            repository,
            ticketPrinter);

        var command = new CompletePurchaseTicketCommand(
            TicketId: 1,
            TareWeight: 10000m,
            Discount: 5m,
            PricePerKg: 2m);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);

        Assert.Equal(
            15000m,
            ticket.NetWeight);

        Assert.Equal(
            750m,
            ticket.DiscountWeight);

        Assert.Equal(
            14250m,
            ticket.NetWeightAfterDiscount);

        Assert.Equal(
            28500m,
            ticket.Amount);

        Assert.NotNull(repository.UpdatedTicket);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.NotNull(ticketPrinter.PrintedTicket);

        Assert.Same(
            repository.UpdatedTicket,
            ticketPrinter.PrintedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.True(result.Printed);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketDoesNotExist()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketPrinter = new FakeTicketPrinter();

        var useCase = new CompletePurchaseTicket(
            repository,
            ticketPrinter);

        var command = new CompletePurchaseTicketCommand(
            TicketId: 999,
            TareWeight: 10000m,
            Discount: 5m,
            PricePerKg: 2m);

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotUpdateTicketWhenCompletionDataIsInvalid()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketPrinter = new FakeTicketPrinter();

        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "abc-123",
            driverName: "juan perez",
            grossWeight: 25000m);

        repository.Ticket = ticket;

        var useCase = new CompletePurchaseTicket(
            repository,
            ticketPrinter);

        var command = new CompletePurchaseTicketCommand(
            TicketId: 1,
            TareWeight: 30000m,
            Discount: 5m,
            PricePerKg: 2m);

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal(
            TicketStatus.Pending,
            ticket.Status);

        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepUpdatedTicketWhenPrintingFails()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();

        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "abc-123",
            driverName: "juan perez",
            grossWeight: 25000m);

        repository.Ticket = ticket;

        var ticketPrinter = new FakeTicketPrinter(
            shouldFail: true);

        var useCase = new CompletePurchaseTicket(
            repository,
            ticketPrinter);

        var command = new CompletePurchaseTicketCommand(
            TicketId: 1,
            TareWeight: 10000m,
            Discount: 5m,
            PricePerKg: 2m);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.UpdatedTicket);

        Assert.Equal(
            TicketStatus.Completed,
            repository.UpdatedTicket.Status);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.False(result.Printed);
    }


}