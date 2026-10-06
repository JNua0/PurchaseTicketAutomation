using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Create;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CreatePurchaseTicketTests
{
    private sealed class FakePurchaseTicketRepository
        : IPurchaseTicketRepository
    {
        public Ticket? AddedTicket { get; private set; }

        public Task AddAsync(Ticket purchaseTicket)
        {
            AddedTicket = purchaseTicket;

            return Task.CompletedTask;
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
            throw new NotImplementedException();
        }
    }

    private sealed class FakeSupplierRepository
        : ISupplierRepository
    {
        private readonly Supplier? _supplier;

        public FakeSupplierRepository(Supplier? supplier)
        {
            _supplier = supplier;
        }

        public Task<Supplier?> GetByIdAsync(int id)
        {
            return Task.FromResult(_supplier);
        }

        public Task AddAsync(Supplier supplier)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeSupplierId = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Supplier supplier)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Supplier>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Supplier>> SearchByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeMaterialRepository
    : IMaterialRepository
    {
        private readonly Material? _material;

        public FakeMaterialRepository(Material? material)
        {
            _material = material;
        }

        public Task<Material?> GetByIdAsync(int id)
        {
            return Task.FromResult(_material);
        }

        public Task AddAsync(Material material)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeMaterialId = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Material material)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Material>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Material>> SearchByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeTicketNumberGenerator
        : ITicketNumberGenerator
    {
        public Task<string> GenerateAsync()
        {
            return Task.FromResult("T-000001");
        }
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
            if (_shouldFail)
                throw new InvalidOperationException(
                    "Printer unavailable.");

            PrintedData = data;

            return Task.CompletedTask;
        }

        public Task PrintFinalAsync(TicketPrintData data)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateAndStorePendingPurchaseTicket()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketNumberGenerator = new FakeTicketNumberGenerator();
        var ticketPrinter = new FakeTicketPrinter();

        var supplier = new Supplier("Proveedor Uno");
        var material = new Material("Acero");
        var supplierRepository = new FakeSupplierRepository(supplier);
        var materialRepository = new FakeMaterialRepository(material);

        var useCase = new CreatePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator,
            ticketPrinter);

        var command = new CreatePurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc-123",
            DriverName: "juan perez",
            GrossWeight: 25000m);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.AddedTicket);

        Assert.Equal(
            "T-000001",
            repository.AddedTicket.TicketNumber);

        Assert.Equal(
            1,
            repository.AddedTicket.SupplierId);

        Assert.Equal(
            2,
            repository.AddedTicket.MaterialId);

        Assert.Equal(
            25000m,
            repository.AddedTicket.GrossWeight);

        Assert.Equal(
            TicketStatus.Pending,
            repository.AddedTicket.Status);

        Assert.Equal(
            "ABC-123",
            repository.AddedTicket.LicensePlate);

        Assert.Equal(
            "Juan Perez",
            repository.AddedTicket.DriverName);

        Assert.NotNull(ticketPrinter.PrintedData);

        Assert.Same(
            repository.AddedTicket,
            ticketPrinter.PrintedData.Ticket);

        Assert.Equal(
            "Proveedor Uno",
            ticketPrinter.PrintedData.SupplierName);

        Assert.Equal(
            "Acero",
            ticketPrinter.PrintedData.MaterialName);

        Assert.True(result.Printed);
        Assert.Equal("T-000001", result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotStoreTicketWhenDataIsInvalid()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketNumberGenerator = new FakeTicketNumberGenerator();
        var ticketPrinter = new FakeTicketPrinter();

        var supplierRepository = new FakeSupplierRepository(new Supplier("Proveedor Uno"));
        var materialRepository = new FakeMaterialRepository(new Material("Acero"));

        var useCase = new CreatePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator,
            ticketPrinter);

        var command = new CreatePurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc-123",
            DriverName: "juan perez",
            GrossWeight: 0m);

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Null(repository.AddedTicket);
        Assert.Null(ticketPrinter.PrintedData);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepStoredTicketWhenPrintingFails()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketNumberGenerator = new FakeTicketNumberGenerator();
        var ticketPrinter = new FakeTicketPrinter(shouldFail: true);

        var supplierRepository = new FakeSupplierRepository(new Supplier("Proveedor Uno"));
        var materialRepository = new FakeMaterialRepository(new Material("Acero"));

        var useCase = new CreatePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator,
            ticketPrinter);

        var command = new CreatePurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc-123",
            DriverName: "juan perez",
            GrossWeight: 25000m);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.AddedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.False(result.Printed);
    }
}