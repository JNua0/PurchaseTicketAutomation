using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class CorrectPurchaseTicketTests
{
    private sealed class FakePurchaseTicketRepository : IPurchaseTicketRepository
    {
        public Ticket? Ticket { get; init; }

        public Ticket? UpdatedTicket { get; private set; }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            return Task.FromResult(Ticket);
        }

        public Task AddAsync(Ticket purchaseTicket)
            => throw new NotImplementedException();

        public Task UpdateAsync(Ticket purchaseTicket)
        {
            UpdatedTicket = purchaseTicket;

            return Task.CompletedTask;
        }

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

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenPurchaseTicketDoesNotExist()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketRepository();

        var supplierRepository =
            new FakeSupplierRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                new FakeMaterialRepository());

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
    public async Task ExecuteAsync_ShouldThrowWhenNewSupplierDoesNotExist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
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

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                new FakeMaterialRepository());

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
    public async Task ExecuteAsync_ShouldThrowWhenNewSupplierIsInactive()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var supplier =
            new Supplier(
                name: "Proveedor Dos",
                phoneNumber: null);

        supplier.Deactivate();

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

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                new FakeMaterialRepository());

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "Supplier is inactive.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeSupplierAndPersist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var supplier =
            new Supplier(
                name: "Proveedor Dos",
                phoneNumber: null);

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

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                new FakeMaterialRepository());

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            2,
            ticket.SupplierId);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            ticket.TicketNumber,
            result.TicketNumber);

        Assert.Equal(
            TicketStatus.WeighingPending,
            ticket.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeSupplierAndMaterialAndPersist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var supplier =
            new Supplier(
                name: "Proveedor Dos",
                phoneNumber: null);

        var material =
            new Material(
                name: "Material Dos");

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

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2,
                MaterialId: 3);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            2,
            ticket.SupplierId);

        Assert.Equal(
            3,
            ticket.MaterialId);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            ticket.TicketNumber,
            result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotModifyTicketWhenMaterialDoesNotExist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var supplier =
            new Supplier(
                name: "Proveedor Dos",
                phoneNumber: null);

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
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2,
                MaterialId: 3);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "Material was not found.",
            exception.Message);

        Assert.Equal(
            1,
            ticket.SupplierId);

        Assert.Equal(
            1,
            ticket.MaterialId);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenNewMaterialIsInactive()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var material =
            new Material(
                name: "Material Dos");

        material.Deactivate();

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository
            {
                Material = material
            };

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                MaterialId: 2);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "Material is inactive.",
            exception.Message);

        Assert.Equal(
            1,
            ticket.MaterialId);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotModifyTicketWhenTransporterIsInvalid()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var supplier =
            new Supplier(
                name: "Proveedor Dos",
                phoneNumber: null);

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
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2,
                Transporter: "123");

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            1,
            ticket.SupplierId);

        Assert.Equal(
            "Juan Perez",
            ticket.Transporter);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeLicensePlateAndPersist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository = new FakeSupplierRepository();

        var materialRepository = new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                ChangeLicensePlate: true,
                LicensePlate: "XYZ789");

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "XYZ789",
            ticket.LicensePlate);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.Equal(
            TicketStatus.WeighingPending,
            ticket.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeTransporterAndPersist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
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

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                Transporter: "pedro lopez");

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "Pedro Lopez",
            ticket.Transporter);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);

        Assert.Equal(
            TicketStatus.WeighingPending,
            ticket.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeGrossWeightAndPersist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
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

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                GrossWeight: 26000m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            26000m,
            ticket.GrossWeight);

        Assert.Null(
            ticket.TareWeight);

        Assert.Null(
            ticket.NetWeight);

        Assert.Equal(
            TicketStatus.WeighingPending,
            ticket.Status);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecalculateAmountsWhenChangingGrossWeightOnCompletedTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);
        ticket.RegisterAmount(
            discount: 10m,
            pricePerKg: 2m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                GrossWeight: 26000m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            26000m,
            ticket.GrossWeight);

        Assert.Equal(
            8000m,
            ticket.TareWeight);

        Assert.Equal(
            18000m,
            ticket.NetWeight);

        Assert.Equal(
            10m,
            ticket.Discount);

        Assert.Equal(
            1800m,
            ticket.DiscountWeight);

        Assert.Equal(
            16200m,
            ticket.NetWeightAfterDiscount);

        Assert.Equal(
            2m,
            ticket.PricePerKg);

        Assert.Equal(
            32400m,
            ticket.Amount);

        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeTareWeightAndPersist()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                TareWeight: 9000m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            25000m,
            ticket.GrossWeight);

        Assert.Equal(
            9000m,
            ticket.TareWeight);

        Assert.Equal(
            16000m,
            ticket.NetWeight);

        Assert.Null(
            ticket.Discount);

        Assert.Null(
            ticket.PricePerKg);

        Assert.Null(
            ticket.Amount);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotModifyWeightsWhenGrossAndTareCombinationIsInvalid()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        ticket.RegisterAmount(
            discount: 10m,
            pricePerKg: 2m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                GrossWeight: 20000m,
                TareWeight: 21000m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "La tara debe ser menor al peso bruto.",
            exception.Message);

        Assert.Equal(
            25000m,
            ticket.GrossWeight);

        Assert.Equal(
            8000m,
            ticket.TareWeight);

        Assert.Equal(
            17000m,
            ticket.NetWeight);

        Assert.Equal(
            15300m,
            ticket.NetWeightAfterDiscount);

        Assert.Equal(
            30600m,
            ticket.Amount);

        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeNetWeightOnSingleTicketAndPersist()
    {
        // Arrange
        var ticket =
            Ticket.CreateSingle(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: null,
                transporter: "Juan Perez",
                netWeight: 15000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                NetWeight: 16000m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Null(
            ticket.GrossWeight);

        Assert.Null(
            ticket.TareWeight);

        Assert.Equal(
            16000m,
            ticket.NetWeight);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeDiscountAndRecalculateAmount()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        ticket.RegisterAmount(
            discount: 10m,
            pricePerKg: 2m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                Discount: 5m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            5m,
            ticket.Discount);

        Assert.Equal(
            850m,
            ticket.DiscountWeight);

        Assert.Equal(
            16150m,
            ticket.NetWeightAfterDiscount);

        Assert.Equal(
            32300m,
            ticket.Amount);

        Assert.Equal(
            2m,
            ticket.PricePerKg);

        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangePricePerKgAndRecalculateAmount()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        ticket.RegisterAmount(
            discount: 10m,
            pricePerKg: 2m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                PricePerKg: 3m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            3m,
            ticket.PricePerKg);

        Assert.Equal(
            17000m,
            ticket.NetWeight);

        Assert.Equal(
            10m,
            ticket.Discount);

        Assert.Equal(
            1700m,
            ticket.DiscountWeight);

        Assert.Equal(
            15300m,
            ticket.NetWeightAfterDiscount);

        Assert.Equal(
            45900m,
            ticket.Amount);

        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenNoCorrectionsAreProvided()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
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

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "No corrections were provided.",
            exception.Message);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenChangingSupplierOnAmountPendingTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        var supplier =
            new Supplier(
                name: "Proveedor Dos",
                phoneNumber: null);

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
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "El ticket no permite modificar este campo en su estado actual.",
            exception.Message);

        Assert.Equal(
            1,
            ticket.SupplierId);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenChangingSupplierOnCancelledTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.Cancel();

        var supplier =
            new Supplier(
                name: "Proveedor Dos",
                phoneNumber: null);

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
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "El ticket no permite modificar este campo en su estado actual.",
            exception.Message);

        Assert.Equal(
            1,
            ticket.SupplierId);

        Assert.Equal(
            TicketStatus.Cancelled,
            ticket.Status);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenChangingGrossWeightOnAmountPendingTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                GrossWeight: 26000m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "El peso bruto no puede modificarse en el estado actual del ticket.",
            exception.Message);

        Assert.Equal(
            25000m,
            ticket.GrossWeight);

        Assert.Equal(
            8000m,
            ticket.TareWeight);

        Assert.Equal(
            17000m,
            ticket.NetWeight);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenChangingTareWeightOnWeighingPendingTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
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

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                TareWeight: 8000m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "La tara no puede modificarse en el estado actual del ticket.",
            exception.Message);

        Assert.Equal(
            25000m,
            ticket.GrossWeight);

        Assert.Null(
            ticket.TareWeight);

        Assert.Null(
            ticket.NetWeight);

        Assert.Equal(
            TicketStatus.WeighingPending,
            ticket.Status);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenChangingNetWeightOnConventionalTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                NetWeight: 16000m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "El peso neto solo puede modificarse directamente en un pesaje único.",
            exception.Message);

        Assert.Equal(
            25000m,
            ticket.GrossWeight);

        Assert.Equal(
            8000m,
            ticket.TareWeight);

        Assert.Equal(
            17000m,
            ticket.NetWeight);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenChangingDiscountOnAmountPendingTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                Discount: 5m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "El ticket debe estar completado.",
            exception.Message);

        Assert.Null(
            ticket.Discount);

        Assert.Null(
            ticket.DiscountWeight);

        Assert.Null(
            ticket.NetWeightAfterDiscount);

        Assert.Null(
            ticket.PricePerKg);

        Assert.Null(
            ticket.Amount);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenChangingPricePerKgOnAmountPendingTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository =
            new FakeSupplierRepository();

        var materialRepository =
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                PricePerKg: 3m);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal(
            "El ticket debe estar completado.",
            exception.Message);

        Assert.Null(
            ticket.PricePerKg);

        Assert.Null(
            ticket.Amount);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Null(
            repository.UpdatedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeSupplierOnCompletedTicket()
    {
        // Arrange
        var ticket =
            Ticket.CreateConventional(
                ticketNumber: "T-000001",
                supplierId: 1,
                materialId: 1,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);

        ticket.RegisterAmount(
            discount: 10m,
            pricePerKg: 2m);

        var supplier =
            new Supplier(
                name: "Proveedor Dos",
                phoneNumber: null);

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
            new FakeMaterialRepository();

        var command =
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2);

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        var result =
            await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            2,
            ticket.SupplierId);

        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);

        Assert.Equal(
            30600m,
            ticket.Amount);

        Assert.Same(
            ticket,
            repository.UpdatedTicket);

        Assert.Equal(
            "T-000001",
            result.TicketNumber);
    }
}