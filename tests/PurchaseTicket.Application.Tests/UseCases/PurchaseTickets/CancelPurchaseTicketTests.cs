using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Cancel;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CancelPurchaseTicketTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCancelPendingPurchaseTicket()
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

        var command = new CancelPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new CancelPurchaseTicket(repository);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(TicketStatus.Cancelled, ticket.Status);
        Assert.Same(ticket, repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketDoesNotExist()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();

        var command = new CancelPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new CancelPurchaseTicket(repository);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "Purchase ticket was not found.",
            exception.Message);

        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotUpdateTicketWhenTicketCannotBeCancelled()
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

        var command = new CancelPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new CancelPurchaseTicket(repository);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(TicketStatus.Completed, ticket.Status);
        Assert.Null(repository.UpdatedTicket);
    }

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
}