using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterAmount;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class RegisterAmountTests
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

        public Task<IReadOnlyList<Ticket>> GetPendingWeighingAsync()
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRegisterAmount()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: null,
            transporter: "juan perez",
            netWeight: 15000m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var useCase =
            new RegisterAmount(repository);

        var command =
            new RegisterAmountCommand(
                TicketId: 1,
                Discount: 5m,
                PricePerKg: 2m);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(5m, ticket.Discount);
        Assert.Equal(2m, ticket.PricePerKg);

        Assert.Equal(
            750m,
            ticket.DiscountWeight);

        Assert.Equal(
            14250m,
            ticket.NetWeightAfterDiscount);

        Assert.Equal(
            28500m,
            ticket.Amount);

        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);

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
            new RegisterAmount(repository);

        var command =
            new RegisterAmountCommand(
                TicketId: 999,
                Discount: 5m,
                PricePerKg: 2m);

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
    public async Task ExecuteAsync_ShouldNotUpdateTicketWhenDiscountIsInvalid()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: null,
            transporter: "juan perez",
            netWeight: 15000m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var useCase =
            new RegisterAmount(repository);

        var command =
            new RegisterAmountCommand(
                TicketId: 1,
                Discount: 100m,
                PricePerKg: 2m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Null(ticket.Discount);
        Assert.Null(ticket.PricePerKg);
        Assert.Null(ticket.Amount);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotUpdateTicketWhenPricePerKgIsInvalid()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: null,
            transporter: "juan perez",
            netWeight: 15000m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var useCase =
            new RegisterAmount(repository);

        var command =
            new RegisterAmountCommand(
                TicketId: 1,
                Discount: 5m,
                PricePerKg: 0m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Null(ticket.Discount);
        Assert.Null(ticket.PricePerKg);
        Assert.Null(ticket.Amount);

        Assert.Null(
            repository.UpdatedTicket);
    }


}