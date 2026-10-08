using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Infrastructure.Persistence.Repositories;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class MaterialQueryRepositoryTests
    : InfrastructureTestBase
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedMaterials()
    {
        Context.Materials.AddRange(
            new Material("Acero"),
            new Material("Cobre"),
            new Material("Aluminio"));

        await Context.SaveChangesAsync();

        var repository =
            new MaterialQueryRepository(Context);

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
        Context.Materials.AddRange(
            new Material("Acero"),
            new Material("Cobre"),
            new Material("Aluminio"));

        await Context.SaveChangesAsync();

        var repository =
            new MaterialQueryRepository(Context);

        var result =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                search: "Cob");

        var material =
            Assert.Single(result.Items);

        Assert.Equal("Cobre", material.Name);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByActiveStatus_WhenIsActiveIsProvided()
    {
        var activeMaterial =
            new Material("Acero");

        var inactiveMaterial =
            new Material("Cobre");

        inactiveMaterial.Deactivate();

        Context.Materials.AddRange(
            activeMaterial,
            inactiveMaterial);

        await Context.SaveChangesAsync();

        var repository =
            new MaterialQueryRepository(Context);

        var result =
            await repository.GetPagedAsync(
                page: 1,
                pageSize: 10,
                isActive: false);

        var material =
            Assert.Single(result.Items);

        Assert.Equal("Cobre", material.Name);
        Assert.False(material.IsActive);
        Assert.Equal(1, result.TotalCount);
    }
}