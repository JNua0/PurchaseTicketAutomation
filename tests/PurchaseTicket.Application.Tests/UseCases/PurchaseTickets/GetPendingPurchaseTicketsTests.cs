using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.GetPending;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class GetPendingPurchaseTicketsTests
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

        public Task<IReadOnlyList<Ticket>> GetPendingAsync()
        {
            return Task.FromResult<IReadOnlyList<Ticket>>(Tickets);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPendingPurchaseTickets()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();

        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierCustomerId: 1,
            materialId: 2,
            licensePlate: "abc-123",
            driverName: "juan perez",
            grossWeight: 25000m);

        repository.Tickets.Add(ticket);

        var useCase =
            new GetPendingPurchaseTickets(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Single(result);

        var item = result[0];

        Assert.IsType<PendingPurchaseTicketDto>(item);

        Assert.Equal(ticket.Id, item.Id);
        Assert.Equal("T-000001", item.TicketNumber);
        Assert.Equal(ticket.CreatedAt, item.CreatedAt);
        Assert.Equal("ABC-123", item.LicensePlate);
        Assert.Equal("Juan Perez", item.DriverName);
        Assert.Equal(25000m, item.GrossWeight);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyListWhenThereAreNoPendingPurchaseTickets()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();

        var useCase =
            new GetPendingPurchaseTickets(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }


}