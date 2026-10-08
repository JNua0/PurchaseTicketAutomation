using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Infrastructure.Persistence.Repositories;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class MaterialRepositoryTests
    : InfrastructureTestBase
{
    [Fact]
    public async Task AddAsync_ShouldPersistMaterial()
    {
        var material =
            new Material("Acero");

        var repository =
            new MaterialRepository(Context);

        await repository.AddAsync(material);

        var persistedMaterial =
            await Context.Materials
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal("Acero", persistedMaterial.Name);
        Assert.True(persistedMaterial.IsActive);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenMaterialExists()
    {
        var material =
            new Material("Acero");

        Context.Materials.Add(material);
        await Context.SaveChangesAsync();

        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.ExistsByNameAsync("Acero");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenMaterialDoesNotExist()
    {
        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.ExistsByNameAsync("Material Inexistente");

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenMatchingMaterialIsExcluded()
    {
        var material =
            new Material("Acero");

        Context.Materials.Add(material);
        await Context.SaveChangesAsync();

        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.ExistsByNameAsync(
                "Acero",
                material.Id);

        Assert.False(result);
    }
}