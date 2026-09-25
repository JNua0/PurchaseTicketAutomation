using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Suppliers.Activate;
using PurchaseTicket.Application.UseCases.Suppliers.Deactivate;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Suppliers;

public class ActivateSupplierTests
{
    private sealed class FakeSupplierRepository
        : ISupplierRepository
    {
        public Supplier? SupplierToReturn { get; set; }

        public Supplier? UpdatedSupplier { get; private set; }

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
    public async Task ExecuteAsync_ShouldActivateExistingSupplier()
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor de prueba",
            "5512345678");

        supplier.Deactivate();

        var repository = new FakeSupplierRepository
        {
            SupplierToReturn = supplier
        };

        var useCase = new ActivateSupplier(repository);

        // Act
        await useCase.ExecuteAsync(supplierId: 1);

        // Assert
        Assert.True(supplier.IsActive);

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

        var useCase = new ActivateSupplier(repository);

        // Act
        async Task Act() => await useCase.ExecuteAsync(supplierId: 999);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.UpdatedSupplier);
    }
}