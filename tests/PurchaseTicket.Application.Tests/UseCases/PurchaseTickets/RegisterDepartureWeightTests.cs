using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterDepartureWeight;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class RegisterDepartureWeightTests
{
    private sealed class FakePurchaseTicketRepository : IPurchaseTicketRepository
    {
        public Ticket? Ticket { get; set; }
        public Ticket? UpdatedTicket { get; private set; }

        public Task AddAsync(Ticket purchaseTicket)
            => throw new NotImplementedException();

        public Task<Ticket?> GetByIdAsync(int id)
        {
            return Task.FromResult(Ticket);
        }

        public Task<IReadOnlyList<Ticket>> GetPendingWeighingAsync()
            => throw new NotImplementedException();

        public Task UpdateAsync(Ticket purchaseTicket)
        {
            UpdatedTicket = purchaseTicket;

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRegisterDepartureWeight()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var ticket = Ticket.CreateConventional(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "abc123",
            transporter: "juan perez",
            grossWeight: 25000m);

        repository.Ticket = ticket;

        var useCase =
            new RegisterDepartureWeight(repository);

        var command =
            new RegisterDepartureWeightCommand(
                TicketId: 1,
                TareWeight: 10000m);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            10000m,
            ticket.TareWeight);

        Assert.Equal(
            15000m,
            ticket.NetWeight);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.NotNull(
            ticket.DepartureAt);

        Assert.NotNull(
            repository.UpdatedTicket);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketDoesNotExist()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var useCase =
            new RegisterDepartureWeight(repository);

        var command =
            new RegisterDepartureWeightCommand(
                TicketId: 999,
                TareWeight: 10000m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Equal(
            "Purchase ticket was not found.",
            exception.Message);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotUpdateTicketWhenTareWeightIsInvalid()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var ticket = Ticket.CreateConventional(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "abc123",
            transporter: "juan perez",
            grossWeight: 25000m);

        repository.Ticket = ticket;

        var useCase =
            new RegisterDepartureWeight(repository);

        var command =
            new RegisterDepartureWeightCommand(
                TicketId: 1,
                TareWeight: 30000m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal(
            TicketStatus.WeighingPending,
            ticket.Status);

        Assert.Null(
            ticket.TareWeight);

        Assert.Null(
            ticket.NetWeight);

        Assert.Null(
            ticket.DepartureAt);

        Assert.Null(
            repository.UpdatedTicket);
    }
}