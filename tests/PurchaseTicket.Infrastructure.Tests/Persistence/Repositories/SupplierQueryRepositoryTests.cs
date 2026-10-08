using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Infrastructure.Persistence.Repositories;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class SupplierQueryRepositoryTests : InfrastructureTestBase
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedSuppliers()
    {
        Context.Suppliers.AddRange(
            new Supplier("Proveedor Uno", "5511111111"),
            new Supplier("Proveedor Dos", "5522222222"),
            new Supplier("Proveedor Tres", "5533333333"));

        await Context.SaveChangesAsync();

        var repository = new SupplierQueryRepository(Context);

        var result =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 2);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByName_WhenSearchIsProvided()
    {
        Context.Suppliers.AddRange(
            new Supplier("Proveedor Uno", "5511111111"),
            new Supplier("Proveedor Dos", "5522222222"),
            new Supplier("Distribuidora Norte", "5533333333"));

        await Context.SaveChangesAsync();

        var repository =
            new SupplierQueryRepository(Context);

        var result =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                search: "Norte");

        var supplier =
            Assert.Single(result.Items);

        Assert.Equal("Distribuidora Norte", supplier.Name);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByActiveStatus_WhenIsActiveIsProvided()
    {
        var activeSupplier =
            new Supplier(
                "Proveedor Activo",
                "5511111111");

        var inactiveSupplier =
            new Supplier(
                "Proveedor Inactivo",
                "5522222222");

        inactiveSupplier.Deactivate();

        Context.Suppliers.AddRange(
            activeSupplier,
            inactiveSupplier);

        await Context.SaveChangesAsync();

        var repository =
            new SupplierQueryRepository(Context);

        var result =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                isActive: false);

        var supplier =
            Assert.Single(result.Items);

        Assert.Equal(
            "Proveedor Inactivo",
            supplier.Name);

        Assert.False(supplier.IsActive);
        Assert.Equal(1, result.TotalCount);
    }
}