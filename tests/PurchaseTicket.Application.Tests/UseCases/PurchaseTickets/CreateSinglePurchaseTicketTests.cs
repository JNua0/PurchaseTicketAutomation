using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateSingle;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CreateSinglePurchaseTicketTests
{
    private sealed class FakePurchaseTicketRepository : IPurchaseTicketRepository
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

        public Task<IReadOnlyList<Ticket>> GetPendingWeighingAsync()
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeSupplierRepository : ISupplierRepository
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

        public Task<IReadOnlyList<Supplier>> GetActiveAsync()
            => throw new NotImplementedException();

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

    private sealed class FakeTicketNumberGenerator : ITicketNumberGenerator
    {
        public Task<string> GenerateAsync()
        {
            return Task.FromResult("T-000001");
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateAndStoreSinglePurchaseTicket()
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

        var useCase = new CreateSinglePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator);

        var command = new CreateSinglePurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc123",
            Transporter: "juan perez",
            NetWeight: 15000m);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

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
            15000m,
            repository.AddedTicket.NetWeight);

        Assert.Equal(
            TicketStatus.AmountPending,
            repository.AddedTicket.Status);

        Assert.Equal(
            WeighingType.Single,
            repository.AddedTicket.WeighingType);

        Assert.Null(
            repository.AddedTicket.GrossWeight);

        Assert.Null(
            repository.AddedTicket.TareWeight);

        Assert.Equal(
            "ABC123",
            repository.AddedTicket.LicensePlate);

        Assert.Equal(
            "Juan Perez",
            repository.AddedTicket.Transporter);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotStoreTicketWhenNetWeightIsInvalid()
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

        var useCase = new CreateSinglePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator);

        var command = new CreateSinglePurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: "abc123",
            Transporter: "juan perez",
            NetWeight: 0m);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Null(repository.AddedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowNullLicensePlateForSinglePurchaseTicket()
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

        var useCase = new CreateSinglePurchaseTicket(
            repository,
            supplierRepository,
            materialRepository,
            ticketNumberGenerator);

        var command = new CreateSinglePurchaseTicketCommand(
            SupplierId: 1,
            MaterialId: 2,
            LicensePlate: null,
            Transporter: "juan perez",
            NetWeight: 15000m);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.AddedTicket);

        Assert.Null(
            repository.AddedTicket.LicensePlate);

        Assert.Equal(
            WeighingType.Single,
            repository.AddedTicket.WeighingType);

        Assert.Equal(
            TicketStatus.AmountPending,
            repository.AddedTicket.Status);
    }


}