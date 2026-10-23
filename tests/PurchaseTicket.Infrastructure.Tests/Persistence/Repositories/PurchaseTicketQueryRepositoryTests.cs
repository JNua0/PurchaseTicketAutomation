using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
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

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByLastWeighingDate()
    {
        // Arrange
        await SeedTicketsAsync();

        var repository =
            new PurchaseTicketQueryRepository(Context);

        var now = DateTime.UtcNow;

        // Act
        var today =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                weighedFrom: now.AddHours(-1),
                weighedTo: now.AddHours(1));

        var tomorrow =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                weighedFrom: now.AddDays(1),
                weighedTo: now.AddDays(2));

        // Assert
        Assert.Equal(2, today.TotalCount);
        Assert.Empty(tomorrow.Items);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldOrderByLastWeighingDescending()
    {
        // Arrange
        await SeedTicketsAsync();

        // La tara del ticket 1 se registra despues de la entrada del 2,
        // asi que su ultimo pesaje es el mas reciente
        var first =
            await Context.PurchaseTickets
                .SingleAsync(t => t.TicketNumber == "1");

        first.RegisterTare(5000m);

        await Context.SaveChangesAsync();

        var repository =
            new PurchaseTicketQueryRepository(Context);

        // Act
        var result =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10);

        // Assert
        Assert.Equal(
            new[] { "1", "2" },
            result.Items.Select(t => t.TicketNumber));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldOrderByLastWeighingAscending_WhenOldestFirst()
    {
        // Arrange
        await SeedTicketsAsync();

        var first =
            await Context.PurchaseTickets
                .SingleAsync(t => t.TicketNumber == "1");

        first.RegisterTare(5000m);

        await Context.SaveChangesAsync();

        var repository =
            new PurchaseTicketQueryRepository(Context);

        // Act
        var result =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                oldestFirst: true);

        // Assert: el 2 lleva mas tiempo desde su ultimo pesaje
        Assert.Equal(
            new[] { "2", "1" },
            result.Items.Select(t => t.TicketNumber));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterBySupplierAndMaterial()
    {
        // Arrange
        await SeedTicketsAsync();

        var south =
            await Context.Suppliers.SingleAsync(s => s.Name == "Proveedor Sur");

        var steel =
            await Context.Materials.SingleAsync(m => m.Name == "Acero");

        var repository =
            new PurchaseTicketQueryRepository(Context);

        // Act
        var bySupplier =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                supplierId: south.Id);

        var byMaterial =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                materialId: steel.Id);

        var both =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                supplierId: south.Id,
                materialId: steel.Id);

        // Assert
        Assert.Equal("2", Assert.Single(bySupplier.Items).TicketNumber);
        Assert.Equal("1", Assert.Single(byMaterial.Items).TicketNumber);
        Assert.Empty(both.Items);
    }

    [Fact]
    public async Task GetDetailAsync_ShouldReturnAllData()
    {
        // Arrange
        await SeedTicketsAsync();

        var first =
            await Context.PurchaseTickets
                .SingleAsync(t => t.TicketNumber == "1");

        first.RegisterTare(5000m);
        first.RegisterAmount(3, 3.33m);

        await Context.SaveChangesAsync();

        var repository =
            new PurchaseTicketQueryRepository(Context);

        // Act
        var detail = await repository.GetDetailAsync(first.Id);

        // Assert: 20,000 kg netos, 600 de descuento, 19,400 x 3.33
        Assert.NotNull(detail);
        Assert.Equal("Proveedor Norte", detail.SupplierName);
        Assert.Equal("Acero", detail.MaterialName);
        Assert.Equal(25000m, detail.GrossWeight);
        Assert.Equal(5000m, detail.TareWeight);
        Assert.Equal(20000m, detail.NetWeight);
        Assert.Equal(3, detail.Discount);
        Assert.Equal(600m, detail.DiscountWeight);
        Assert.Equal(19400m, detail.NetWeightAfterDiscount);
        Assert.Equal(3.33m, detail.PricePerKg);
        Assert.Equal(64602m, detail.Amount);
        Assert.Equal(TicketStatus.Completed, detail.Status);
        Assert.NotNull(detail.DepartureAt);
    }

    [Fact]
    public async Task GetDetailAsync_ShouldReturnNullWhenTicketDoesNotExist()
    {
        // Arrange
        var repository =
            new PurchaseTicketQueryRepository(Context);

        // Act
        var detail = await repository.GetDetailAsync(999);

        // Assert
        Assert.Null(detail);
    }

    [Fact]
    public async Task GetSuppliersAndMaterialsInUseAsync_ShouldIncludeInactiveWithTickets()
    {
        // Arrange: el seed crea tickets para Norte/Acero y Sur/Cobre
        await SeedTicketsAsync();

        var unused = new Supplier("Proveedor Sin Tickets");
        var unusedMaterial = new Material("Vidrio");

        Context.Suppliers.Add(unused);
        Context.Materials.Add(unusedMaterial);

        var south =
            await Context.Suppliers.SingleAsync(s => s.Name == "Proveedor Sur");

        south.Deactivate();

        await Context.SaveChangesAsync();

        var repository =
            new PurchaseTicketQueryRepository(Context);

        // Act
        var suppliers = await repository.GetSuppliersInUseAsync();
        var materials = await repository.GetMaterialsInUseAsync();

        // Assert: sin los que no tienen tickets, ordenados y con su estado
        Assert.Equal(
            new[] { "Proveedor Norte", "Proveedor Sur" },
            suppliers.Select(s => s.Name));

        Assert.False(suppliers[1].IsActive);

        Assert.Equal(
            new[] { "Acero", "Cobre" },
            materials.Select(m => m.Name));
    }

    [Fact]
    public async Task GetDayTotalsAsync_ShouldCountTodayWeighingsWithoutCancelled()
    {
        // Arrange
        var supplier = new Supplier("Proveedor Norte");
        var material = new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        // En planta: cuenta como pesaje, pero aun no tiene peso neto
        var inPlant = Ticket.CreateConventional("1", supplier.Id, material.Id, "ABC123", "Juan Perez", 25000m);

        var withTare = Ticket.CreateConventional("2", supplier.Id, material.Id, "XYZ789", "Pedro Lopez", 25000m);
        withTare.RegisterTare(10000m);

        var single = Ticket.CreateSingle("3", supplier.Id, material.Id, null, "Juan Perez", 500m);

        var cancelled = Ticket.CreateSingle("4", supplier.Id, material.Id, null, "Juan Perez", 900m);
        cancelled.Cancel();

        Context.PurchaseTickets.AddRange(inPlant, withTare, single, cancelled);

        await Context.SaveChangesAsync();

        var repository = new PurchaseTicketQueryRepository(Context);

        var now = DateTime.UtcNow;

        // Act
        var today = await repository.GetDayTotalsAsync(now.AddHours(-1), now.AddHours(1));
        var tomorrow = await repository.GetDayTotalsAsync(now.AddDays(1), now.AddDays(2));

        // Assert
        Assert.Equal(3, today.TicketCount);
        Assert.Equal(2, today.ConventionalCount);
        Assert.Equal(1, today.SingleCount);
        Assert.Equal(15500m, today.NetWeight);

        Assert.Equal(0, tomorrow.TicketCount);
        Assert.Equal(0m, tomorrow.NetWeight);
    }
}
