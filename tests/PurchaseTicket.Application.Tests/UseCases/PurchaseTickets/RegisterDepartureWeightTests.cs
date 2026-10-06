using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Complete;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CompletePurchaseTicketTests
{
    private sealed class FakePurchaseTicketRepository
        : IPurchaseTicketRepository
    {
        public Ticket? Ticket { get; set; }
        public Ticket? UpdatedTicket { get; private set; }

        public Task AddAsync(Ticket purchaseTicket)
            => throw new NotImplementedException();

        public Task<Ticket?> GetByIdAsync(int id)
        {
            return Task.FromResult(Ticket);
        }

        public Task<IReadOnlyList<Ticket>> GetPendingAsync()
            => throw new NotImplementedException();

        public Task UpdateAsync(Ticket purchaseTicket)
        {
            UpdatedTicket = purchaseTicket;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Ticket>> GetAllAsync()
            => throw new NotImplementedException();
    }

    private sealed class FakeSupplierRepository
        : ISupplierRepository
    {
        public Supplier? Supplier { get; init; }

        public Task<Supplier?> GetByIdAsync(int id)
        {
            return Task.FromResult(Supplier);
        }

        public Task AddAsync(Supplier supplier)
            => throw new NotImplementedException();

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeSupplierId = null)
            => throw new NotImplementedException();

        public Task UpdateAsync(Supplier supplier)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Supplier>> GetAllAsync()
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Supplier>> SearchByNameAsync(
            string name)
            => throw new NotImplementedException();
    }

    private sealed class FakeMaterialRepository
        : IMaterialRepository
    {
        public Material? Material { get; init; }

        public Task<Material?> GetByIdAsync(int id)
        {
            return Task.FromResult(Material);
        }

        public Task AddAsync(Material material)
            => throw new NotImplementedException();

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeMaterialId = null)
            => throw new NotImplementedException();

        public Task UpdateAsync(Material material)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Material>> GetAllAsync()
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Material>> SearchByNameAsync(
            string name)
            => throw new NotImplementedException();
    }

    private sealed class FakeTicketPrinter : ITicketPrinter
    {
        private readonly bool _shouldFail;

        public TicketPrintData? PrintedData { get; private set; }

        public FakeTicketPrinter(bool shouldFail = false)
        {
            _shouldFail = shouldFail;
        }

        public Task PrintInitialAsync(TicketPrintData data)
        {
            throw new NotImplementedException();
        }

        public Task PrintFinalAsync(TicketPrintData data)
        {
            if (_shouldFail)
                throw new InvalidOperationException(
                    "Printer unavailable.");

            PrintedData = data;

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCompletePendingPurchaseTicket()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();

        var supplierRepository = new FakeSupplierRepository
        {
            Supplier = new Supplier("Proveedor Uno")
        };

        var materialRepository = new FakeMaterialRepository
        {
            Material = new Material("Acero")
        };

        var ticketPrinter = new FakeTicketPrinter();

        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "abc-123",
            driverName: "juan perez",
            grossWeight: 25000m);

        repository.Ticket = ticket;

        var useCase = new CompletePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketPrinter);

        var command = new CompletePurchaseTicketCommand(
            TicketId: 1,
            TareWeight: 10000m,
            Discount: 5m,
            PricePerKg: 2m);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);

        Assert.Equal(
            15000m,
            ticket.NetWeight);

        Assert.Equal(
            750m,
            ticket.DiscountWeight);

        Assert.Equal(
            14250m,
            ticket.NetWeightAfterDiscount);

        Assert.Equal(
            28500m,
            ticket.Amount);

        Assert.NotNull(repository.UpdatedTicket);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.NotNull(ticketPrinter.PrintedData);

        Assert.Same(
            ticket,
            ticketPrinter.PrintedData.Ticket);

        Assert.Equal(
            "Proveedor Uno",
            ticketPrinter.PrintedData.SupplierName);

        Assert.Equal(
            "Acero",
            ticketPrinter.PrintedData.MaterialName);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.True(result.Printed);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketDoesNotExist()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();

        var supplierRepository = new FakeSupplierRepository
        {
            Supplier = new Supplier("Proveedor Uno")
        };

        var materialRepository = new FakeMaterialRepository
        {
            Material = new Material("Acero")
        };

        var ticketPrinter = new FakeTicketPrinter();

        var useCase = new CompletePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketPrinter);

        var command = new CompletePurchaseTicketCommand(
            TicketId: 999,
            TareWeight: 10000m,
            Discount: 5m,
            PricePerKg: 2m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.UpdatedTicket);
        Assert.Null(ticketPrinter.PrintedData);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotUpdateTicketWhenCompletionDataIsInvalid()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();

        var supplierRepository = new FakeSupplierRepository
        {
            Supplier = new Supplier("Proveedor Uno")
        };

        var materialRepository = new FakeMaterialRepository
        {
            Material = new Material("Acero")
        };

        var ticketPrinter = new FakeTicketPrinter();

        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "abc-123",
            driverName: "juan perez",
            grossWeight: 25000m);

        repository.Ticket = ticket;

        var useCase = new CompletePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketPrinter);

        var command = new CompletePurchaseTicketCommand(
            TicketId: 1,
            TareWeight: 30000m,
            Discount: 5m,
            PricePerKg: 2m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal(
            TicketStatus.Pending,
            ticket.Status);

        Assert.Null(repository.UpdatedTicket);
        Assert.Null(ticketPrinter.PrintedData);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepUpdatedTicketWhenPrintingFails()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();

        var supplierRepository = new FakeSupplierRepository
        {
            Supplier = new Supplier("Proveedor Uno")
        };

        var materialRepository = new FakeMaterialRepository
        {
            Material = new Material("Acero")
        };

        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "abc-123",
            driverName: "juan perez",
            grossWeight: 25000m);

        repository.Ticket = ticket;

        var ticketPrinter = new FakeTicketPrinter(
            shouldFail: true);

        var useCase = new CompletePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketPrinter);

        var command = new CompletePurchaseTicketCommand(
            TicketId: 1,
            TareWeight: 10000m,
            Discount: 5m,
            PricePerKg: 2m);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.UpdatedTicket);

        Assert.Equal(
            TicketStatus.Completed,
            repository.UpdatedTicket.Status);

        Assert.Null(ticketPrinter.PrintedData);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.False(result.Printed);
    }
}