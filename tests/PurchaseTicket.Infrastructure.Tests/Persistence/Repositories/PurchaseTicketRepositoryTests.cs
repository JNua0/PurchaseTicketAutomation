using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Infrastructure.Persistence.Repositories;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class PurchaseTicketRepositoryTests : InfrastructureTestBase
{
    [Fact]
    public async Task AddAsync_ShouldPersistPurchaseTicket()
    {
        // Arrange
        var supplier =
            new Supplier(
                "Proveedor Uno",
                null);

        var material =
            new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var purchaseTicket =
            Ticket.CreateConventional(
                ticketNumber: "T-0001",
                supplierId: supplier.Id,
                materialId: material.Id,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 10000m);

        var repository =
            new PurchaseTicketRepository(Context);

        // Act
        await repository.AddAsync(purchaseTicket);

        Context.ChangeTracker.Clear();

        // Assert
        var persistedTicket =
            await Context.PurchaseTickets
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(
            "T-0001",
            persistedTicket.TicketNumber);

        Assert.Equal(
            supplier.Id,
            persistedTicket.SupplierId);

        Assert.Equal(
            material.Id,
            persistedTicket.MaterialId);

        Assert.Equal(
            "ABC123",
            persistedTicket.LicensePlate);

        Assert.Equal(
            "Juan Perez",
            persistedTicket.Transporter);

        Assert.Equal(
            10000m,
            persistedTicket.GrossWeight);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPurchaseTicket_WhenTicketExists()
    {
        // Arrange
        var supplier =
            new Supplier(
                "Proveedor Uno",
                null);

        var material =
            new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var purchaseTicket =
            Ticket.CreateConventional(
                ticketNumber: "T-0001",
                supplierId: supplier.Id,
                materialId: material.Id,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 10000m);

        Context.PurchaseTickets.Add(purchaseTicket);
        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var repository =
            new PurchaseTicketRepository(Context);

        // Act
        var result =
            await repository.GetByIdAsync(
                purchaseTicket.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            purchaseTicket.Id,
            result.Id);

        Assert.Equal(
            "T-0001",
            result.TicketNumber);

        Assert.Equal(
            "Juan Perez",
            result.Transporter);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTicketDoesNotExist()
    {
        // Arrange
        var repository =
            new PurchaseTicketRepository(Context);

        // Act
        var result =
            await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistPurchaseTicketChanges()
    {
        // Arrange
        var supplier =
            new Supplier(
                "Proveedor Uno",
                null);

        var material =
            new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var purchaseTicket =
            Ticket.CreateConventional(
                ticketNumber: "T-0001",
                supplierId: supplier.Id,
                materialId: material.Id,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 10000m);

        Context.PurchaseTickets.Add(purchaseTicket);
        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var repository =
            new PurchaseTicketRepository(Context);

        var ticketToUpdate =
            await repository.GetByIdAsync(
                purchaseTicket.Id);

        Assert.NotNull(ticketToUpdate);

        ticketToUpdate.RegisterTare(
            tareWeight: 4000m);

        // Act
        await repository.UpdateAsync(
            ticketToUpdate);

        Context.ChangeTracker.Clear();

        // Assert
        var persistedTicket =
            await Context.PurchaseTickets
                .AsNoTracking()
                .SingleAsync(ticket =>
                    ticket.Id == purchaseTicket.Id);

        Assert.Equal(
            4000m,
            persistedTicket.TareWeight);

        Assert.Equal(
            6000m,
            persistedTicket.NetWeight);

        Assert.NotNull(
            persistedTicket.DepartureAt);
    }

    [Fact]
    public async Task GetPendingWeighingAsync_ShouldReturnOnlyWeighingPendingTickets()
    {
        // Arrange
        var supplier =
            new Supplier(
                "Proveedor Uno",
                null);

        var material =
            new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var weighingPendingTicket =
            Ticket.CreateConventional(
                ticketNumber: "T-0001",
                supplierId: supplier.Id,
                materialId: material.Id,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 10000m);

        var amountPendingTicket =
            Ticket.CreateSingle(
                ticketNumber: "T-0002",
                supplierId: supplier.Id,
                materialId: material.Id,
                licensePlate: null,
                transporter: "Pedro Lopez",
                netWeight: 12000m);

        Context.PurchaseTickets.AddRange(
            weighingPendingTicket,
            amountPendingTicket);

        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var repository =
            new PurchaseTicketRepository(Context);

        // Act
        var result =
            await repository.GetPendingWeighingAsync();

        // Assert
        Assert.Single(result);

        Assert.Equal(
            weighingPendingTicket.Id,
            result[0].Id);

        Assert.Equal(
            "T-0001",
            result[0].TicketNumber);
    }
}