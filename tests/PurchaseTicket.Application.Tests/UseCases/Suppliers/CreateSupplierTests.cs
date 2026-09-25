using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Suppliers.Create;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Suppliers;

public class CreateSupplierTests
{
    private sealed class FakeSupplierRepository
        : ISupplierRepository
    {
        public Supplier? AddedSupplier { get; private set; }

        public bool NameExists { get; set; }

        public string? CheckedName { get; private set; }

        public Task AddAsync(Supplier supplier)
        {
            AddedSupplier = supplier;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeSupplierId = null)
        {
            CheckedName = name;
            return Task.FromResult(NameExists);
        }

        public Task<Supplier?> GetByIdAsync(int id)
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

    [Fact]
    public async Task ExecuteAsync_ShouldCreateAndStoreActiveSupplier()
    {
        // Arrange
        var repository = new FakeSupplierRepository();

        var useCase = new CreateSupplier(repository);

        var command = new CreateSupplierCommand(
            Name: "Proveedor de prueba",
            PhoneNumber: "5512345678");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.AddedSupplier);

        Assert.Equal(
            "Proveedor de prueba",
            repository.AddedSupplier.Name);

        Assert.Equal(
            "5512345678",
            repository.AddedSupplier.PhoneNumber);

        Assert.True(repository.AddedSupplier.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotStoreSupplierWhenDataIsInvalid()
    {
        // Arrange
        var repository = new FakeSupplierRepository();

        var useCase = new CreateSupplier(repository);

        var command = new CreateSupplierCommand(
            Name: "",
            PhoneNumber: "5512345678");

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Null(repository.AddedSupplier);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotStoreSupplierWhenNameAlreadyExists()
    {
        // Arrange
        var repository = new FakeSupplierRepository
        {
            NameExists = true
        };

        var useCase = new CreateSupplier(repository);

        var command = new CreateSupplierCommand(
            Name: "Proveedor existente",
            PhoneNumber: "5512345678");

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.AddedSupplier);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCheckExistenceUsingNormalizedName()
    {
        // Arrange
        var repository = new FakeSupplierRepository();

        var useCase = new CreateSupplier(repository);

        var command = new CreateSupplierCommand(
            Name: "   Proveedor de prueba   ",
            PhoneNumber: "5512345678");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "Proveedor de prueba",
            repository.CheckedName);
    }
}