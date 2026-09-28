using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Infrastructure.Persistence.Repositories;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class SupplierRepositoryTests : InfrastructureTestBase
{
    [Fact]
    public async Task AddAsync_ShouldPersistSupplier()
    {
        var repository = new SupplierRepository(Context);

        var supplier = new Supplier(
            "Proveedor Uno",
            "5512345678");

        await repository.AddAsync(supplier);

        var persistedSupplier =
            await Context.Suppliers
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal("Proveedor Uno", persistedSupplier.Name);
        Assert.Equal("5512345678", persistedSupplier.PhoneNumber);
        Assert.True(persistedSupplier.IsActive);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenSupplierExists()
    {
        var supplier = new Supplier(
            "Proveedor Uno",
            "5512345678");

        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        var repository = new SupplierRepository(Context);

        var exists =
            await repository.ExistsByNameAsync("Proveedor Uno");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenSupplierDoesNotExist()
    {
        var repository = new SupplierRepository(Context);

        var exists =
            await repository.ExistsByNameAsync("Proveedor Inexistente");

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenMatchingSupplierIsExcluded()
    {
        var supplier = new Supplier(
            "Proveedor Uno",
            "5512345678");

        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        var repository = new SupplierRepository(Context);

        var exists =
            await repository.ExistsByNameAsync(
                "Proveedor Uno",
                supplier.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenAnotherSupplierHasMatchingName()
    {
        var supplier = new Supplier(
            "Proveedor Uno",
            "5512345678");

        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        var repository = new SupplierRepository(Context);

        var exists =
            await repository.ExistsByNameAsync(
                "Proveedor Uno",
                supplier.Id + 1);

        Assert.True(exists);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSupplier_WhenSupplierExists()
    {
        var supplier = new Supplier(
            "Proveedor Uno",
            "5512345678");

        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        var repository = new SupplierRepository(Context);

        var result =
            await repository.GetByIdAsync(supplier.Id);

        Assert.NotNull(result);
        Assert.Equal(supplier.Id, result.Id);
        Assert.Equal("Proveedor Uno", result.Name);
        Assert.Equal("5512345678", result.PhoneNumber);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSupplierDoesNotExist()
    {
        var repository = new SupplierRepository(Context);

        var result =
            await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistSupplierChanges()
    {
        var supplier = new Supplier(
            "Proveedor Uno",
            "5512345678");

        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        supplier.UpdateName("Proveedor Actualizado");
        supplier.UpdatePhoneNumber("5587654321");

        var repository = new SupplierRepository(Context);

        await repository.UpdateAsync(supplier);

        Context.ChangeTracker.Clear();

        var persistedSupplier =
            await Context.Suppliers
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal("Proveedor Actualizado", persistedSupplier.Name);
        Assert.Equal("5587654321", persistedSupplier.PhoneNumber);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSuppliers()
    {
        var supplierOne = new Supplier(
            "Proveedor Uno",
            "5512345678");

        var supplierTwo = new Supplier(
            "Proveedor Dos",
            "5587654321");

        Context.Suppliers.AddRange(
            supplierOne,
            supplierTwo);

        await Context.SaveChangesAsync();

        var repository = new SupplierRepository(Context);

        var result =
            await repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(
            result,
            supplier => supplier.Name == "Proveedor Uno");
        Assert.Contains(
            result,
            supplier => supplier.Name == "Proveedor Dos");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoSuppliersExist()
    {
        var repository = new SupplierRepository(Context);

        var result =
            await repository.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnSuppliers_WithPartialNameMatch()
    {
        var supplierOne = new Supplier(
            "Proveedor Norte",
            "5512345678");

        var supplierTwo = new Supplier(
            "Proveedor Sur",
            "5587654321");

        var supplierThree = new Supplier(
            "Comercializadora Centro",
            null);

        Context.Suppliers.AddRange(
            supplierOne,
            supplierTwo,
            supplierThree);

        await Context.SaveChangesAsync();

        var repository = new SupplierRepository(Context);

        var result =
            await repository.SearchByNameAsync("Proveedor");

        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            supplier => supplier.Name == "Proveedor Norte");

        Assert.Contains(
            result,
            supplier => supplier.Name == "Proveedor Sur");
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldBeCaseInsensitive()
    {
        var supplier = new Supplier(
            "Proveedor Norte",
            "5512345678");

        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        var repository = new SupplierRepository(Context);

        var result =
            await repository.SearchByNameAsync("proveedor norte");

        Assert.Single(result);
        Assert.Equal(
            "Proveedor Norte",
            result[0].Name);
    }
}