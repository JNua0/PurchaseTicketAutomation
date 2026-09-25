using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Suppliers.Get;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Suppliers;

public class GetSuppliersTests
{
    private sealed class FakeSupplierRepository
        : ISupplierRepository
    {
        public IReadOnlyList<Supplier> SuppliersToReturn { get; set; }
            = [];

        public Task AddAsync(Supplier supplier)
        {
            throw new NotImplementedException();
        }

        public string? SearchName { get; private set; }

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeSupplierId = null)
        {
            throw new NotImplementedException();
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
            return Task.FromResult(SuppliersToReturn);
        }

        public Task<IReadOnlyList<Supplier>> SearchByNameAsync(string name)
        {
            SearchName = name;

            IReadOnlyList<Supplier> result = SuppliersToReturn
                .Where(supplier =>
                    supplier.Name.Contains(
                        name,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(result);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnRegisteredSuppliers()
    {
        // Arrange
        var supplier1 = new Supplier(
            "Proveedor uno",
            "5512345678");

        var supplier2 = new Supplier(
            "Proveedor dos");

        supplier2.Deactivate();

        var repository = new FakeSupplierRepository
        {
            SuppliersToReturn =
            [
                supplier1,
                supplier2
            ]
        };

        var useCase = new GetSuppliers(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Contains(supplier1, result);
        Assert.Contains(supplier2, result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyListWhenNoSuppliersExist()
    {
        // Arrange
        var repository = new FakeSupplierRepository
        {
            SuppliersToReturn = []
        };

        var useCase = new GetSuppliers(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuppliersMatchingName()
    {
        // Arrange
        var supplier1 = new Supplier(
            "Aceros del norte");

        var supplier2 = new Supplier(
            "Proveedor de acero");

        var supplier3 = new Supplier(
            "Transportes del norte");

        var repository = new FakeSupplierRepository
        {
            SuppliersToReturn =
            [
                supplier1,
            supplier2,
            supplier3
            ]
        };

        var useCase = new GetSuppliers(repository);

        // Act
        var result = await useCase.ExecuteAsync("acero");

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Contains(supplier1, result);
        Assert.Contains(supplier2, result);
        Assert.DoesNotContain(supplier3, result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyListWhenNoSuppliersMatchName()
    {
        // Arrange
        var supplier1 = new Supplier(
            "Aceros del norte");

        var supplier2 = new Supplier(
            "Proveedor de acero");

        var repository = new FakeSupplierRepository
        {
            SuppliersToReturn =
            [
                supplier1,
            supplier2
            ]
        };

        var useCase = new GetSuppliers(repository);

        // Act
        var result = await useCase.ExecuteAsync("cemento");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldTrimSearchTerm()
    {
        // Arrange
        var repository = new FakeSupplierRepository();

        var useCase = new GetSuppliers(repository);

        // Act
        await useCase.ExecuteAsync("   acero   ");

        // Assert
        Assert.Equal(
            "acero",
            repository.SearchName);
    }
}