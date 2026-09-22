using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class GetPurchaseTicketsTests
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
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Ticket>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<Ticket>>(Tickets);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPurchaseTickets()
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

        repository.Tickets.Add(ticket);

        var useCase =
            new GetPurchaseTickets(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Single(result);

        var item = result[0];

        Assert.IsType<PurchaseTicketDto>(item);

        Assert.Equal(ticket.Id, item.Id);
        Assert.Equal("T-000001", item.TicketNumber);
        Assert.Equal(ticket.CreatedAt, item.CreatedAt);
        Assert.Equal(1, item.SupplierId);
        Assert.Equal(2, item.MaterialId);
        Assert.Equal("ABC-123", item.LicensePlate);
        Assert.Equal("Juan Perez", item.DriverName);
        Assert.Equal(25000m, item.GrossWeight);
        Assert.Null(item.TareWeight);
        Assert.Null(item.NetWeight);
        Assert.Null(item.Discount);
        Assert.Null(item.DiscountWeight);
        Assert.Null(item.NetWeightAfterDiscount);
        Assert.Null(item.PricePerKg);
        Assert.Null(item.Amount);
        Assert.Equal("Pending", item.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyListWhenThereAreNoPurchaseTickets()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var useCase = new GetPurchaseTickets(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }


}