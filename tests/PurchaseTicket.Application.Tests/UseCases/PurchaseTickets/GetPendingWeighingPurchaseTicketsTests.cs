using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.GetPendingWeighing;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class GetPendingWeighingPurchaseTicketsTests
{
    private sealed class FakePurchaseTicketRepository
        : IPurchaseTicketRepository
    {
        public List<Ticket> Tickets { get; } = [];

        public Task AddAsync(Ticket purchaseTicket)
        {
            throw new NotImplementedException();
        }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Ticket purchaseTicket)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Ticket>> GetPendingWeighingAsync()
        {
            return Task.FromResult<IReadOnlyList<Ticket>>(Tickets);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPendingWeighingPurchaseTickets()
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

        repository.Tickets.Add(ticket);

        var useCase =
            new GetPendingWeighingPurchaseTickets(repository);

        // Act
        var result =
            await useCase.ExecuteAsync();

        // Assert
        Assert.Single(result);

        var item = result[0];

        Assert.IsType<PendingWeighingPurchaseTicketDto>(item);

        Assert.Equal(
            "T-000001",
            item.TicketNumber);

        Assert.Equal(
            ticket.CheckInAt,
            item.CheckInAt);

        Assert.Equal(
            "ABC123",
            item.LicensePlate);

        Assert.Equal(
            "Juan Perez",
            item.Transporter);

        Assert.Equal(
            25000m,
            item.GrossWeight);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyListWhenThereAreNoPendingWeighingPurchaseTickets()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var useCase =
            new GetPendingWeighingPurchaseTickets(repository);

        // Act
        var result =
            await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}