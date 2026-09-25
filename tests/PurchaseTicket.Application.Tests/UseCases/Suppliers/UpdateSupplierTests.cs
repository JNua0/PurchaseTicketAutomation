using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Suppliers.Update;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Suppliers;

public class UpdateSupplierTests
{
    private sealed class FakeSupplierRepository
        : ISupplierRepository
    {
        public Supplier? SupplierToReturn { get; set; }

        public Supplier? UpdatedSupplier { get; private set; }

        public bool NameExists { get; set; }

        public int? ExcludedSupplierId { get; private set; }

        public string? CheckedName { get; private set; }

        public Task AddAsync(Supplier supplier)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeSupplierId = null)
        {
            CheckedName = name;
            ExcludedSupplierId = excludeSupplierId;

            return Task.FromResult(NameExists);
        }

        public Task<Supplier?> GetByIdAsync(int id)
        {
            return Task.FromResult(SupplierToReturn);
        }

        public Task UpdateAsync(Supplier supplier)
        {
            UpdatedSupplier = supplier;
            return Task.CompletedTask;
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

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateExistingSupplier()
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor anterior",
            "5511111111");

        var repository = new FakeSupplierRepository
        {
            SupplierToReturn = supplier
        };

        var useCase = new UpdateSupplier(repository);

        var command = new UpdateSupplierCommand(
            SupplierId: 1,
            Name: "Proveedor actualizado",
            PhoneNumber: "5522222222");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "Proveedor actualizado",
            supplier.Name);

        Assert.Equal(
            "5522222222",
            supplier.PhoneNumber);

        Assert.Same(
            supplier,
            repository.UpdatedSupplier);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenSupplierDoesNotExist()
    {
        // Arrange
        var repository = new FakeSupplierRepository
        {
            SupplierToReturn = null
        };

        var useCase = new UpdateSupplier(repository);

        var command = new UpdateSupplierCommand(
            SupplierId: 999,
            Name: "Proveedor actualizado",
            PhoneNumber: "5522222222");

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.UpdatedSupplier);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotUpdateSupplierWhenNameAlreadyExists()
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor anterior",
            "5511111111");

        var repository = new FakeSupplierRepository
        {
            SupplierToReturn = supplier,
            NameExists = true
        };

        var useCase = new UpdateSupplier(repository);

        var command = new UpdateSupplierCommand(
            SupplierId: 1,
            Name: "Proveedor existente",
            PhoneNumber: "5522222222");

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.UpdatedSupplier);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExcludeCurrentSupplierWhenCheckingName()
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor anterior",
            "5511111111");

        var repository = new FakeSupplierRepository
        {
            SupplierToReturn = supplier
        };

        var useCase = new UpdateSupplier(repository);

        var command = new UpdateSupplierCommand(
            SupplierId: 1,
            Name: "Proveedor actualizado",
            PhoneNumber: "5522222222");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            command.SupplierId,
            repository.ExcludedSupplierId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCheckExistenceUsingNormalizedName()
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor anterior",
            "5511111111");

        var repository = new FakeSupplierRepository
        {
            SupplierToReturn = supplier
        };

        var useCase = new UpdateSupplier(repository);

        var command = new UpdateSupplierCommand(
            SupplierId: 1,
            Name: "   Proveedor actualizado   ",
            PhoneNumber: "5522222222");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "Proveedor actualizado",
            repository.CheckedName);
    }
}