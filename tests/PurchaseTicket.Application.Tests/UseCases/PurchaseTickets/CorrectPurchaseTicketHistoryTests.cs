using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

// El historial que deja CorregirTicket: proveedor y material por nombre,
// el motivo y ningun registro si no cambio nada
public class CorrectPurchaseTicketHistoryTests
{
    private sealed class FakePurchaseTicketRepository : IPurchaseTicketRepository
    {
        public Ticket? Ticket { get; init; }

        public int UpdateCount { get; private set; }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            return Task.FromResult(Ticket);
        }

        public Task AddAsync(Ticket purchaseTicket)
            => throw new NotImplementedException();

        public Task UpdateAsync(Ticket purchaseTicket)
        {
            UpdateCount++;

            return Task.CompletedTask;
        }
    }

    // Devuelve el proveedor segun su id, para distinguir el anterior del nuevo
    private sealed class FakeSupplierRepository : ISupplierRepository
    {
        public Dictionary<int, Supplier> Suppliers { get; } = [];

        public Task AddAsync(Supplier supplier)
            => throw new NotImplementedException();

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeSupplierId = null)
            => throw new NotImplementedException();

        public Task<Supplier?> GetByIdAsync(int id)
        {
            return Task.FromResult(Suppliers.GetValueOrDefault(id));
        }

        public Task UpdateAsync(Supplier supplier)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Supplier>> GetActiveAsync()
            => throw new NotImplementedException();
    }

    private sealed class FakeMaterialRepository : IMaterialRepository
    {
        public Dictionary<int, Material> Materials { get; } = [];

        public Task AddAsync(Material material)
            => throw new NotImplementedException();

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeMaterialId = null)
            => throw new NotImplementedException();

        public Task<Material?> GetByIdAsync(int id)
        {
            return Task.FromResult(Materials.GetValueOrDefault(id));
        }

        public Task UpdateAsync(Material material)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Material>> GetActiveAsync()
            => throw new NotImplementedException();
    }

    private static Ticket CreateCompletedTicket()
    {
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
            discount: 10,
            pricePerKg: 2m);

        return ticket;
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecordSupplierAndMaterialNames()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var supplierRepository = new FakeSupplierRepository();
        supplierRepository.Suppliers[1] = new Supplier("Proveedor Uno");
        supplierRepository.Suppliers[2] = new Supplier("Proveedor Dos");

        var materialRepository = new FakeMaterialRepository();
        materialRepository.Materials[1] = new Material("Carton");
        materialRepository.Materials[2] = new Material("Archivo");

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                supplierRepository,
                materialRepository);

        // Act
        await useCase.ExecuteAsync(
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                SupplierId: 2,
                MaterialId: 2));

        // Assert
        var correction = Assert.Single(ticket.Corrections);

        Assert.Collection(
            correction.Changes,
            supplier =>
            {
                Assert.Equal(TicketField.Supplier, supplier.Field);
                Assert.Equal("Proveedor Uno", supplier.Before);
                Assert.Equal("Proveedor Dos", supplier.After);
            },
            material =>
            {
                Assert.Equal(TicketField.Material, material.Field);
                Assert.Equal("Carton", material.Before);
                Assert.Equal("Archivo", material.After);
            });

        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecordReason()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                new FakeSupplierRepository(),
                new FakeMaterialRepository());

        // Act
        await useCase.ExecuteAsync(
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                TareWeight: 8100m,
                Reason: " La tara se capturo antes de que bajara el chofer "));

        // Assert
        var correction = Assert.Single(ticket.Corrections);

        Assert.Equal(
            "La tara se capturo antes de que bajara el chofer",
            correction.Reason);

        Assert.Equal(
            new[] { TicketField.TareWeight, TicketField.NetWeight, TicketField.Amount },
            correction.Changes.Select(change => change.Field).ToArray());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotRecordCorrectionWhenNothingChanged()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                new FakeSupplierRepository(),
                new FakeMaterialRepository());

        // Act
        await useCase.ExecuteAsync(
            new CorrectPurchaseTicketCommand(
                TicketId: 1,
                TareWeight: 8000m));

        // Assert
        Assert.Empty(ticket.Corrections);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenReasonIsTooLongWithoutChangingTicket()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        var repository =
            new FakePurchaseTicketRepository
            {
                Ticket = ticket
            };

        var useCase =
            new CorrectPurchaseTicket(
                repository,
                new FakeSupplierRepository(),
                new FakeMaterialRepository());

        // Act
        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(
                new CorrectPurchaseTicketCommand(
                    TicketId: 1,
                    TareWeight: 8100m,
                    Reason: new string('a', TicketCorrection.ReasonMaxLength + 1))));

        // Assert
        Assert.Equal(8000m, ticket.TareWeight);
        Assert.Empty(ticket.Corrections);
        Assert.Equal(0, repository.UpdateCount);
    }
}
