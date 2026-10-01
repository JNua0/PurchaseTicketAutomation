using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;
using PurchaseTicket.Domain.Entities;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CorrectPurchaseTicketTests
{
    private sealed class FakeTicketPrinter : ITicketPrinter
    {
        public TicketPrintData? PrintedData { get; private set; }
        public bool ShouldThrow { get; init; }

        public Task PrintInitialAsync(TicketPrintData data)
        {
            if (ShouldThrow)
                throw new InvalidOperationException(
                    "Printer error.");

            PrintedData = data;

            return Task.CompletedTask;
        }

        public Task PrintFinalAsync(TicketPrintData data)
        {
            throw new NotImplementedException();
        }
    }

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

        public Task<IReadOnlyList<Ticket>> GetPendingAsync()
            => throw new NotImplementedException();

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

    [Fact]
    public async Task ExecuteAsync_ShouldCorrectPendingPurchaseTicket()
    {
        // Arrange
        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "ABC-123",
            driverName: "Juan Perez",
            grossWeight: 25000m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var supplierRepository = new FakeSupplierRepository
        {
            Supplier = new Supplier("Proveedor Tres")
        };

        var materialRepository = new FakeMaterialRepository
        {
            Material = new Material("Aluminio")
        };

        var printer = new FakeTicketPrinter();

        var command = new CorrectPurchaseTicketCommand(
            TicketId: 1,
            SupplierId: 3,
            MaterialId: 4,
            LicensePlate: "xyz-789",
            DriverName: "pedro lopez",
            GrossWeight: 28000m);

        var useCase = new CorrectPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            printer);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(3, ticket.SupplierId);
        Assert.Equal(4, ticket.MaterialId);
        Assert.Equal("XYZ-789", ticket.LicensePlate);
        Assert.Equal("Pedro Lopez", ticket.DriverName);
        Assert.Equal(28000m, ticket.GrossWeight);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.NotNull(printer.PrintedData);

        Assert.Same(
            ticket,
            printer.PrintedData.Ticket);

        Assert.Equal(
            "Proveedor Tres",
            printer.PrintedData.SupplierName);

        Assert.Equal(
            "Aluminio",
            printer.PrintedData.MaterialName);

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
            Supplier = new Supplier("Proveedor Tres")
        };

        var materialRepository = new FakeMaterialRepository
        {
            Material = new Material("Aluminio")
        };

        var printer = new FakeTicketPrinter();

        var command = new CorrectPurchaseTicketCommand(
            TicketId: 1,
            SupplierId: 3,
            MaterialId: 4,
            LicensePlate: "XYZ-789",
            DriverName: "Pedro Lopez",
            GrossWeight: 28000m);

        var useCase = new CorrectPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            printer);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "Purchase ticket was not found.",
            exception.Message);

        Assert.Null(repository.UpdatedTicket);
        Assert.Null(printer.PrintedData);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotUpdateTicketWhenTicketCannotBeCorrected()
    {
        // Arrange
        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "ABC-123",
            driverName: "Juan Perez",
            grossWeight: 25000m);

        ticket.Complete(
            tareWeight: 10000m,
            discount: 5m,
            pricePerKg: 2m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var supplierRepository = new FakeSupplierRepository
        {
            Supplier = new Supplier("Proveedor Tres")
        };

        var materialRepository = new FakeMaterialRepository
        {
            Material = new Material("Aluminio")
        };

        var printer = new FakeTicketPrinter();

        var command = new CorrectPurchaseTicketCommand(
            TicketId: 1,
            SupplierId: 3,
            MaterialId: 4,
            LicensePlate: "XYZ-789",
            DriverName: "Pedro Lopez",
            GrossWeight: 28000m);

        var useCase = new CorrectPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            printer);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Null(repository.UpdatedTicket);
        Assert.Null(printer.PrintedData);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepUpdatedTicketWhenPrintingFails()
    {
        // Arrange
        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "ABC-123",
            driverName: "Juan Perez",
            grossWeight: 25000m);

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var supplierRepository = new FakeSupplierRepository
        {
            Supplier = new Supplier("Proveedor Tres")
        };

        var materialRepository = new FakeMaterialRepository
        {
            Material = new Material("Aluminio")
        };

        var printer = new FakeTicketPrinter
        {
            ShouldThrow = true
        };

        var command = new CorrectPurchaseTicketCommand(
            TicketId: 1,
            SupplierId: 3,
            MaterialId: 4,
            LicensePlate: "XYZ-789",
            DriverName: "Pedro Lopez",
            GrossWeight: 28000m);

        var useCase = new CorrectPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            printer);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(3, ticket.SupplierId);
        Assert.Equal(4, ticket.MaterialId);
        Assert.Equal("XYZ-789", ticket.LicensePlate);
        Assert.Equal("Pedro Lopez", ticket.DriverName);
        Assert.Equal(28000m, ticket.GrossWeight);

        Assert.Null(printer.PrintedData);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.False(result.Printed);
    }
}