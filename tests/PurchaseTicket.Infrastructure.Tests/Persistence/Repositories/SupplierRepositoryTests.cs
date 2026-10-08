using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Infrastructure.Persistence.Repositories;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class SupplierRepositoryTests
    : InfrastructureTestBase
{
    [Fact]
    public async Task AddAsync_ShouldPersistSupplier()
    {
        var supplier =
            new Supplier(
                "Proveedor Uno",
                "5512345678");

        var repository =
            new SupplierRepository(Context);

        await repository.AddAsync(supplier);

        var persistedSupplier =
            await Context.Suppliers
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(
            "Proveedor Uno",
            persistedSupplier.Name);

        Assert.Equal(
            "5512345678",
            persistedSupplier.PhoneNumber);

        Assert.True(
            persistedSupplier.IsActive);
    }
}