using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateConventional;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.Exceptions;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CreateConventionalPurchaseTicketTests
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

        public Task<IReadOnlyList<Supplier>> GetActiveAsync()
            => throw new NotImplementedException();

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
    }

    private sealed class FakeMaterialRepository : IMaterialRepository
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

        public Task<IReadOnlyList<Material>> GetActiveAsync()
            => throw new NotImplementedException();

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
    }

    private sealed class FakeTicketNumberGenerator
        : ITicketNumberGenerator
    {
        public Task<string> GenerateAsync()
        {
            return Task.FromResult("T-000001");
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateAndStoreConventionalPurchaseTicket()
    {
        // Arrange
        var repository = new FakePurchaseTicketRepository();
        var ticketNumberGenerator = new FakeTicketNumberGenerator();

        var supplier = new Supplier("Proveedor Uno");
        var material = new Material("Acero");

        var supplierRepository =
            new FakeSupplierRepository(supplier);

        var materialRepository =
            new FakeMaterialRepository(material);

        var useCase = new CreateConventionalPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator);

        var command = new CreateConventionalPurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc123",
            Transporter: "juan perez",
            GrossWeight: 25000m);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.AddedTicket);

        Assert.Equal(
            "T-000001",
            repository.AddedTicket.TicketNumber);

        Assert.Equal(1, repository.AddedTicket.SupplierId);

        Assert.Equal(2, repository.AddedTicket.MaterialId);

        Assert.Equal(25000m, repository.AddedTicket.GrossWeight);

        Assert.Equal(TicketStatus.WeighingPending, repository.AddedTicket.Status);

        Assert.Equal(WeighingType.Conventional, repository.AddedTicket.WeighingType);

        Assert.Equal("ABC123", repository.AddedTicket.LicensePlate);

        Assert.Equal("Juan Perez", repository.AddedTicket.Transporter);

        Assert.Equal("T-000001", result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotStoreTicketWhenGrossWeightIsInvalid()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var ticketNumberGenerator =
            new FakeTicketNumberGenerator();

        var supplierRepository =
            new FakeSupplierRepository(
                new Supplier("Proveedor Uno"));

        var materialRepository =
            new FakeMaterialRepository(
                new Material("Acero"));

        var useCase = new CreateConventionalPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator);

        var command = new CreateConventionalPurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc123",
            Transporter: "juan perez",
            GrossWeight: 0m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Null(repository.AddedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenSupplierDoesNotExist()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var supplierRepository =
            new FakeSupplierRepository(null);

        var materialRepository =
            new FakeMaterialRepository(
                new Material("Acero"));

        var ticketNumberGenerator =
            new FakeTicketNumberGenerator();

        var useCase = new CreateConventionalPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator);

        var command = new CreateConventionalPurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc123",
            Transporter: "juan perez",
            GrossWeight: 25000m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        var exception =
            await Assert.ThrowsAsync<NotFoundException>(Act);

        Assert.Equal(
            "No se encontró el proveedor.",
            exception.Message);

        Assert.Null(repository.AddedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenMaterialDoesNotExist()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var supplierRepository =
            new FakeSupplierRepository(
                new Supplier("Proveedor Uno"));

        var materialRepository =
            new FakeMaterialRepository(null);

        var ticketNumberGenerator =
            new FakeTicketNumberGenerator();

        var useCase = new CreateConventionalPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator);

        var command = new CreateConventionalPurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc123",
            Transporter: "juan perez",
            GrossWeight: 25000m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        var exception =
            await Assert.ThrowsAsync<NotFoundException>(Act);

        Assert.Equal(
            "No se encontró el material.",
            exception.Message);

        Assert.Null(repository.AddedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenSupplierIsInactive()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var supplier = new Supplier("Proveedor Uno");
        supplier.Deactivate();

        var supplierRepository =
            new FakeSupplierRepository(supplier);

        var materialRepository =
            new FakeMaterialRepository(
                new Material("Acero"));

        var useCase = new CreateConventionalPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            new FakeTicketNumberGenerator());

        var command = new CreateConventionalPurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc123",
            Transporter: "juan perez",
            GrossWeight: 25000m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        var exception =
            await Assert.ThrowsAsync<BusinessRuleException>(Act);

        Assert.Equal(
            "El proveedor seleccionado está inactivo.",
            exception.Message);

        Assert.Null(repository.AddedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenMaterialIsInactive()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var material = new Material("Acero");
        material.Deactivate();

        var supplierRepository =
            new FakeSupplierRepository(
                new Supplier("Proveedor Uno"));

        var materialRepository =
            new FakeMaterialRepository(material);

        var useCase = new CreateConventionalPurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            new FakeTicketNumberGenerator());

        var command = new CreateConventionalPurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc123",
            Transporter: "juan perez",
            GrossWeight: 25000m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        var exception =
            await Assert.ThrowsAsync<BusinessRuleException>(Act);

        Assert.Equal(
            "El material seleccionado está inactivo.",
            exception.Message);

        Assert.Null(repository.AddedTicket);
    }
}