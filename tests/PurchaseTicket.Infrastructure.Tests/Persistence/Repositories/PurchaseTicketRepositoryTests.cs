using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Infrastructure.Persistence.Repositories;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class PurchaseTicketRepositoryTests : InfrastructureTestBase
{
    [Fact]
    public async Task AddAsync_ShouldPersistPurchaseTicket()
    {
        var supplier =
            new Supplier("Proveedor Uno", null);

        var material =
            new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var purchaseTicket =
            new Ticket(
                "T-0001",
                supplier.Id,
                material.Id,
                "abc-123",
                "juan perez",
                10000m);

        var repository =
            new PurchaseTicketRepository(Context);

        await repository.AddAsync(purchaseTicket);

        Context.ChangeTracker.Clear();

        var persistedTicket =
            await Context.PurchaseTickets
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal("T-0001", persistedTicket.TicketNumber);
        Assert.Equal(supplier.Id, persistedTicket.SupplierId);
        Assert.Equal(material.Id, persistedTicket.MaterialId);
        Assert.Equal("ABC-123", persistedTicket.LicensePlate);
        Assert.Equal("Juan Perez", persistedTicket.DriverName);
        Assert.Equal(10000m, persistedTicket.GrossWeight);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAtInUtc()
    {
        var ticket =
            new Ticket(
                "T-0001",
                1,
                1,
                "ABC-123",
                "Juan Perez",
                10000m);

        Assert.Equal(
            DateTimeKind.Utc,
            ticket.CreatedAt.Kind);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPurchaseTicket_WhenTicketExists()
    {
        var supplier =
            new Supplier("Proveedor Uno", null);

        var material =
            new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var purchaseTicket =
            new Ticket(
                "T-0001",
                supplier.Id,
                material.Id,
                "ABC-123",
                "Juan Perez",
                10000m);

        Context.PurchaseTickets.Add(purchaseTicket);
        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var repository =
            new PurchaseTicketRepository(Context);

        var result =
            await repository.GetByIdAsync(purchaseTicket.Id);

        Assert.NotNull(result);
        Assert.Equal(purchaseTicket.Id, result.Id);
        Assert.Equal("T-0001", result.TicketNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTicketDoesNotExist()
    {
        var repository =
            new PurchaseTicketRepository(Context);

        var result =
            await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistPurchaseTicketChanges()
    {
        var supplier =
            new Supplier("Proveedor Uno", null);

        var material =
            new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var purchaseTicket =
            new Ticket(
                "T-0001",
                supplier.Id,
                material.Id,
                "ABC-123",
                "Juan Perez",
                10000m);

        Context.PurchaseTickets.Add(purchaseTicket);
        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var repository =
            new PurchaseTicketRepository(Context);

        var ticketToUpdate =
            await repository.GetByIdAsync(purchaseTicket.Id);

        Assert.NotNull(ticketToUpdate);

        ticketToUpdate.Correct(
            supplier.Id,
            material.Id,
            "XYZ-789",
            "Pedro Lopez",
            12000m);

        await repository.UpdateAsync(ticketToUpdate);

        Context.ChangeTracker.Clear();

        var persistedTicket =
            await Context.PurchaseTickets
                .AsNoTracking()
                .SingleAsync(ticket =>
                    ticket.Id == purchaseTicket.Id);

        Assert.Equal("XYZ-789", persistedTicket.LicensePlate);
        Assert.Equal("Pedro Lopez", persistedTicket.DriverName);
        Assert.Equal(12000m, persistedTicket.GrossWeight);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnOnlyPendingTickets()
    {
        var supplier =
            new Supplier("Proveedor Uno", null);

        var material =
            new Material("Acero");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var pendingTicket =
            new Ticket(
                "T-0001",
                supplier.Id,
                material.Id,
                "ABC-123",
                "Juan Perez",
                10000m);

        var completedTicket =
            new Ticket(
                "T-0002",
                supplier.Id,
                material.Id,
                "XYZ-789",
                "Pedro Lopez",
                12000m);

        completedTicket.Complete(
            5000m,
            0m,
            10m);

        Context.PurchaseTickets.AddRange(
            pendingTicket,
            completedTicket);

        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var repository =
            new PurchaseTicketRepository(Context);

        var result =
            await repository.GetPendingAsync();

        Assert.Single(result);
        Assert.Equal(pendingTicket.Id, result[0].Id);
        Assert.Equal("T-0001", result[0].TicketNumber);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnEmptyList_WhenNoPendingTicketsExist()
    {
        var repository =
            new PurchaseTicketRepository(Context);

        var result =
            await repository.GetPendingAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPurchaseTickets()
    {
        var supplier = new Supplier("Proveedor Uno", null);
        var material = new Material("Material Uno");

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);
        await Context.SaveChangesAsync();

        var pendingTicket = new Ticket(
            "TICKET-001",
            supplier.Id,
            material.Id,
            "ABC123",
            "Juan Perez",
            1000m);

        var completedTicket = new Ticket(
            "TICKET-002",
            supplier.Id,
            material.Id,
            "DEF456",
            "Pedro Lopez",
            1200m);

        completedTicket.Complete(
            500m,
            0m,
            10m);

        Context.PurchaseTickets.AddRange(
            pendingTicket,
            completedTicket);

        await Context.SaveChangesAsync();

        var repository =
            new PurchaseTicketRepository(Context);

        var result =
            await repository.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPurchaseTicketsExist()
    {
        var repository =
            new PurchaseTicketRepository(Context);

        var result =
            await repository.GetAllAsync();

        Assert.Empty(result);
    }
}