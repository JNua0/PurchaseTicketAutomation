using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Infrastructure.Persistence.Repositories;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class PurchaseTicketQueryRepositoryTests : InfrastructureTestBase
{
    // Dos tickets con datos distintos en cada campo de busqueda
    private async Task SeedTicketsAsync()
    {
        var supplierNorth = new Supplier("Proveedor Norte");
        var supplierSouth = new Supplier("Proveedor Sur");

        var steel = new Material("Acero");
        var copper = new Material("Cobre");

        Context.Suppliers.AddRange(supplierNorth, supplierSouth);
        Context.Materials.AddRange(steel, copper);

        await Context.SaveChangesAsync();

        Context.PurchaseTickets.AddRange(
            Ticket.CreateConventional(
                ticketNumber: "1",
                supplierId: supplierNorth.Id,
                materialId: steel.Id,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m),
            Ticket.CreateConventional(
                ticketNumber: "2",
                supplierId: supplierSouth.Id,
                materialId: copper.Id,
                licensePlate: "XYZ789",
                transporter: "Pedro Lopez",
                grossWeight: 18000m));

        await Context.SaveChangesAsync();
    }

    [Theory]
    [InlineData("abc", "1")]
    [InlineData("juan", "1")]
    [InlineData("NORTE", "1")]
    [InlineData("cobre", "2")]
    public async Task GetPagedAsync_ShouldSearchIgnoringCase(
        string search,
        string expectedTicketNumber)
    {
        // Arrange
        await SeedTicketsAsync();

        var repository =
            new PurchaseTicketQueryRepository(Context);

        // Act
        var result =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                search: search);

        // Assert
        var ticket = Assert.Single(result.Items);

        Assert.Equal(expectedTicketNumber, ticket.TicketNumber);
    }
}