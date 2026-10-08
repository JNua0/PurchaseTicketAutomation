using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Print;
using PurchaseTicket.Domain.Entities;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class PrintPurchaseTicketTests
{
    private sealed class FakePurchaseTicketRepository
        : IPurchaseTicketRepository
    {
        public Ticket? Ticket { get; init; }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            return Task.FromResult(Ticket);
        }

        public Task AddAsync(Ticket purchaseTicket)
            => throw new NotImplementedException();

        public Task UpdateAsync(Ticket purchaseTicket)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Ticket>> GetPendingWeighingAsync()
            => throw new NotImplementedException();
    }

    private sealed class FakeSupplierRepository : ISupplierRepository
    {
        public Supplier? Supplier { get; init; }

        public Task AddAsync(Supplier supplier)
            => throw new NotImplementedException();

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeSupplierId = null)
            => throw new NotImplementedException();

        public Task<Supplier?> GetByIdAsync(int id)
        {
            return Task.FromResult(Supplier);
        }

        public Task UpdateAsync(Supplier supplier)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Supplier>> GetActiveAsync()
            => throw new NotImplementedException();
    }

    private sealed class FakeMaterialRepository : IMaterialRepository
    {
        public Material? Material { get; init; }

        public Task AddAsync(Material material)
            => throw new NotImplementedException();

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeMaterialId = null)
            => throw new NotImplementedException();

        public Task<Material?> GetByIdAsync(int id)
        {
            return Task.FromResult(Material);
        }

        public Task UpdateAsync(Material material)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Material>> GetActiveAsync()
            => throw new NotImplementedException();
    }

    private sealed class FakeTicketPrinter : ITicketPrinter
    {
        public TicketPrintData? ConventionalInitialData { get; private set; }
        public TicketPrintData? ConventionalFinalData { get; private set; }
        public TicketPrintData? ConventionalCompletedData { get; private set; }
        public TicketPrintData? SingleData { get; private set; }
        public bool ThrowOnConventionalInitial { get; init; }

        public Task PrintConventionalInitialAsync(TicketPrintData data)
        {
            if (ThrowOnConventionalInitial)
                throw new InvalidOperationException("Printer error.");

            ConventionalInitialData = data;

            return Task.CompletedTask;
        }

        public Task PrintConventionalFinalAsync(TicketPrintData data)
        {
            ConventionalFinalData = data;
            return Task.CompletedTask;
        }

        public Task PrintConventionalCompletedAsync(TicketPrintData data)
        {
            ConventionalCompletedData = data;
            return Task.CompletedTask;
        }

        public Task PrintSingleAsync(TicketPrintData data)
        {
            SingleData = data;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketDoesNotExist()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var printer =
            new FakeTicketPrinter();

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
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
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenSupplierDoesNotExist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 2,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var printer =
            new FakeTicketPrinter();

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
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
            "Supplier was not found.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenMaterialDoesNotExist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 2,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository
            {
                Supplier = new Supplier(
                    name: "Proveedor Uno",
                    phoneNumber: null)
            };

        var materialRepository =
            new FakeMaterialRepository();

        var printer =
            new FakeTicketPrinter();

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
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
            "Material was not found.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPrintConventionalInitialWhenWeighingIsPending()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 2,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var supplier =
            new Supplier(
                name: "Proveedor Uno",
                phoneNumber: null);

        var material =
            new Material(
                name: "Material Uno");

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository
            {
                Supplier = supplier
            };

        var materialRepository =
            new FakeMaterialRepository
            {
                Material = material
            };

        var printer =
            new FakeTicketPrinter();

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository,
                printer);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            ticket.TicketNumber,
            result.TicketNumber);

        Assert.True(result.Printed);

        Assert.NotNull(
            printer.ConventionalInitialData);

        Assert.Same(
            ticket,
            printer.ConventionalInitialData.Ticket);

        Assert.Equal(
            supplier.Name,
            printer.ConventionalInitialData.SupplierName);

        Assert.Equal(
            material.Name,
            printer.ConventionalInitialData.MaterialName);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPrintConventionalFinalWhenAmountIsPending()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 2,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(
            tareWeight: 10000m);

        var supplier =
            new Supplier(
                name: "Proveedor Uno",
                phoneNumber: null);

        var material =
            new Material(
                name: "Material Uno");

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository
            {
                Supplier = supplier
            };

        var materialRepository =
            new FakeMaterialRepository
            {
                Material = material
            };

        var printer =
            new FakeTicketPrinter();

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository,
                printer);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            ticket.TicketNumber,
            result.TicketNumber);

        Assert.True(result.Printed);

        Assert.NotNull(
            printer.ConventionalFinalData);

        Assert.Same(
            ticket,
            printer.ConventionalFinalData.Ticket);

        Assert.Equal(
            supplier.Name,
            printer.ConventionalFinalData.SupplierName);

        Assert.Equal(
            material.Name,
            printer.ConventionalFinalData.MaterialName);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPrintSingleWhenAmountIsPending()
    {
        // Arrange
        var ticket =
            Ticket.CreateSingle(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 2,
                licensePlate: null,
                transporter: "Juan Perez",
                netWeight: 15000m);

        var supplier =
            new Supplier(
                name: "Proveedor Uno",
                phoneNumber: null);

        var material =
            new Material(
                name: "Material Uno");

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository
            {
                Supplier = supplier
            };

        var materialRepository =
            new FakeMaterialRepository
            {
                Material = material
            };

        var printer =
            new FakeTicketPrinter();

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository,
                printer);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            ticket.TicketNumber,
            result.TicketNumber);

        Assert.True(result.Printed);

        Assert.NotNull(
            printer.SingleData);

        Assert.Same(
            ticket,
            printer.SingleData.Ticket);

        Assert.Equal(
            supplier.Name,
            printer.SingleData.SupplierName);

        Assert.Equal(
            material.Name,
            printer.SingleData.MaterialName);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPrintCompletedConventionalTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 2,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(
            tareWeight: 10000m);

        ticket.RegisterAmount(
            discount: 5m,
            pricePerKg: 2m);

        var supplier =
            new Supplier(
                name: "Proveedor Uno",
                phoneNumber: null);

        var material =
            new Material(
                name: "Material Uno");

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository
            {
                Supplier = supplier
            };

        var materialRepository =
            new FakeMaterialRepository
            {
                Material = material
            };

        var printer =
            new FakeTicketPrinter();

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository,
                printer);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            ticket.TicketNumber,
            result.TicketNumber);

        Assert.True(result.Printed);

        Assert.NotNull(
            printer.ConventionalCompletedData);

        Assert.Same(
            ticket,
            printer.ConventionalCompletedData.Ticket);

        Assert.Equal(
            supplier.Name,
            printer.ConventionalCompletedData.SupplierName);

        Assert.Equal(
            material.Name,
            printer.ConventionalCompletedData.MaterialName);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketCannotBePrinted()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 2,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.Cancel();

        var supplier =
            new Supplier(
                name: "Proveedor Uno",
                phoneNumber: null);

        var material =
            new Material(
                name: "Material Uno");

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository
            {
                Supplier = supplier
            };

        var materialRepository =
            new FakeMaterialRepository
            {
                Material = material
            };

        var printer =
            new FakeTicketPrinter();

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
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
            "Purchase ticket cannot be printed in its current state.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPrintedFalseWhenPrinterFails()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 2,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var supplier =
            new Supplier(
                name: "Proveedor Uno",
                phoneNumber: null);

        var material =
            new Material(
                name: "Material Uno");

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository
            {
                Supplier = supplier
            };

        var materialRepository =
            new FakeMaterialRepository
            {
                Material = material
            };

        var printer =
            new FakeTicketPrinter
            {
                ThrowOnConventionalInitial = true
            };

        var command =
            new PrintPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new PrintPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository,
                printer);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            ticket.TicketNumber,
            result.TicketNumber);

        Assert.False(result.Printed);
    }
}