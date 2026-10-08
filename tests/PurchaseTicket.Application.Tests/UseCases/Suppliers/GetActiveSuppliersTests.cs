using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Suppliers.GetActive;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Suppliers;

public class GetActiveSuppliersTests
{
    private sealed class FakeSupplierRepository
        : ISupplierRepository
    {
        public IReadOnlyList<Supplier> ActiveSuppliersToReturn { get; set; }
            = [];

        public Task<IReadOnlyList<Supplier>> GetActiveAsync()
        {
            return Task.FromResult(ActiveSuppliersToReturn);
        }

        public Task AddAsync(Supplier supplier)
            => throw new NotImplementedException();

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeSupplierId = null)
            => throw new NotImplementedException();

        public Task<Supplier?> GetByIdAsync(int id)
            => throw new NotImplementedException();

        public Task UpdateAsync(Supplier supplier)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Supplier>> GetAllAsync()
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Supplier>> SearchByNameAsync(string name)
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyListWhenThereAreNoActiveSuppliers()
    {
        // Arrange
        var repository = new FakeSupplierRepository();

        var useCase = new GetActiveSuppliers(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSupplierOptions()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            new("Aceros del Norte", "5512345678"),
            new("Metales del Centro")
        };

        var repository = new FakeSupplierRepository
        {
            ActiveSuppliersToReturn = suppliers
        };

        var useCase = new GetActiveSuppliers(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("Aceros del Norte", result[0].Name);
        Assert.Equal("Metales del Centro", result[1].Name);
    }
}