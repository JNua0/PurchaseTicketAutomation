using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Reprint;
using PurchaseTicket.Domain.Entities;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class ReprintPurchaseTicketTests
{
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

    private sealed class FakeTicketPrinter : ITicketPrinter
    {
        public TicketPrintData? PrintedData { get; private set; }
        public bool ThrowOnPrint { get; init; }

        public Task PrintInitialAsync(TicketPrintData data)
        {
            throw new NotImplementedException();
        }

        public Task PrintFinalAsync(TicketPrintData data)
        {
            if (ThrowOnPrint)
                throw new InvalidOperationException(
                    "Printer error.");

            PrintedData = data;

            return Task.CompletedTask;
        }
    }

    private static FakeSupplierRepository CreateSupplierRepository()
    {
        return new FakeSupplierRepository
        {
            Supplier = new Supplier("Proveedor Uno")
        };
    }

    private static FakeMaterialRepository CreateMaterialRepository()
    {
        return new FakeMaterialRepository
        {
            Material = new Material("Acero")
        };
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReprintCompletedPurchaseTicket()
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

        var supplierRepository =
            CreateSupplierRepository();

        var materialRepository =
            CreateMaterialRepository();

        var printer = new FakeTicketPrinter();

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new ReprintPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            printer);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(printer.PrintedData);

        Assert.Same(
            ticket,
            printer.PrintedData.Ticket);

        Assert.Equal(
            "Proveedor Uno",
            printer.PrintedData.SupplierName);

        Assert.Equal(
            "Acero",
            printer.PrintedData.MaterialName);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.True(result.Printed);

        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketDoesNotExist()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository
        {
            Ticket = null
        };

        var supplierRepository =
            CreateSupplierRepository();

        var materialRepository =
            CreateMaterialRepository();

        var printer = new FakeTicketPrinter();

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 999);

        var useCase = new ReprintPurchaseTicket(
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

        Assert.Null(printer.PrintedData);
        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketIsNotCompleted()
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

        var supplierRepository =
            CreateSupplierRepository();

        var materialRepository =
            CreateMaterialRepository();

        var printer = new FakeTicketPrinter();

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new ReprintPurchaseTicket(
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
            "Only completed purchase tickets can be reprinted.",
            exception.Message);

        Assert.Null(printer.PrintedData);
        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketIsCancelled()
    {
        // Arrange
        var ticket = new Ticket(
            ticketNumber: "T-000001",
            supplierId: 1,
            materialId: 2,
            licensePlate: "ABC-123",
            driverName: "Juan Perez",
            grossWeight: 25000m);

        ticket.Cancel();

        var repository = new FakePurchaseTicketRepository
        {
            Ticket = ticket
        };

        var supplierRepository =
            CreateSupplierRepository();

        var materialRepository =
            CreateMaterialRepository();

        var printer = new FakeTicketPrinter();

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new ReprintPurchaseTicket(
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
            "Only completed purchase tickets can be reprinted.",
            exception.Message);

        Assert.Null(printer.PrintedData);
        Assert.Null(repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPrintedFalseWhenPrinterFails()
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

        var supplierRepository =
            CreateSupplierRepository();

        var materialRepository =
            CreateMaterialRepository();

        var printer = new FakeTicketPrinter
        {
            ThrowOnPrint = true
        };

        var command = new ReprintPurchaseTicketCommand(
            TicketId: 1);

        var useCase = new ReprintPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            printer);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.False(result.Printed);

        Assert.Null(printer.PrintedData);
        Assert.Null(repository.UpdatedTicket);
    }
}