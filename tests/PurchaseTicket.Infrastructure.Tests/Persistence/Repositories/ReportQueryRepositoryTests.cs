using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Infrastructure.Persistence.Repositories;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class ReportQueryRepositoryTests : InfrastructureTestBase
{
    // Un ticket de cada caso; las fechas de pesaje son las de ahora
    private async Task SeedTicketsAsync()
    {
        var supplier = new Supplier("Proveedor Uno");
        var material = new Material("Carton");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        Ticket Conventional(string number) =>
            Ticket.CreateConventional(number, supplier.Id, material.Id, "ABC123", "Juan Perez", 25000m);

        var inPlant = Conventional("1");

        var amountPending = Conventional("2");
        amountPending.RegisterTare(8000m);

        var completed = Conventional("3");
        completed.RegisterTare(5000m);
        completed.RegisterAmount(discount: 10, pricePerKg: 2m);

        var cancelled = Conventional("4");
        cancelled.Cancel();

        var single = Ticket.CreateSingle("5", supplier.Id, material.Id, null, "Juan Perez", 12000m);

        Context.PurchaseTickets.AddRange(inPlant, amountPending, completed, cancelled, single);

        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetTicketsAsync_ShouldReturnCompletedAndAmountPendingOnly()
    {
        // Arrange
        await SeedTicketsAsync();

        var repository = new ReportQueryRepository(Context);

        // Act
        var tickets = await repository.GetTicketsAsync(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1));

        // Assert: sin el vehiculo en planta ni el cancelado
        Assert.Equal(
            new[] { "2", "3", "5" },
            tickets.Select(ticket => ticket.TicketNumber).OrderBy(number => number).ToArray());

        var completed = Assert.Single(tickets, ticket => ticket.Status == TicketStatus.Completed);
        Assert.Equal(20000m, completed.NetWeight);
        Assert.Equal(36000m, completed.Amount);
        Assert.Equal("Proveedor Uno", completed.SupplierName);
        Assert.True(completed.IsSupplierActive);

        var single = Assert.Single(tickets, ticket => ticket.TicketNumber == "5");
        Assert.Null(single.Amount);
        Assert.Equal(12000m, single.NetWeight);
    }

    [Fact]
    public async Task GetTicketsAsync_ShouldExcludeTicketsOutsideThePeriod()
    {
        // Arrange
        await SeedTicketsAsync();

        var repository = new ReportQueryRepository(Context);

        // Act
        var tickets = await repository.GetTicketsAsync(
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2));

        // Assert
        Assert.Empty(tickets);
    }
}
