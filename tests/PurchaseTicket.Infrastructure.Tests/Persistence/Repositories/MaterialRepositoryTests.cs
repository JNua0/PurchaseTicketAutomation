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

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenAnotherMaterialHasMatchingName()
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
                material.Id + 1);

        Assert.True(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMaterial_WhenMaterialExists()
    {
        var material =
            new Material("Acero");

        Context.Materials.Add(material);
        await Context.SaveChangesAsync();

        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.GetByIdAsync(material.Id);

        Assert.NotNull(result);
        Assert.Equal(material.Id, result.Id);
        Assert.Equal("Acero", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMaterialDoesNotExist()
    {
        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistMaterialChanges()
    {
        var material =
            new Material("Acero");

        Context.Materials.Add(material);
        await Context.SaveChangesAsync();

        material.UpdateName("Aluminio");

        var repository =
            new MaterialRepository(Context);

        await repository.UpdateAsync(material);

        Context.ChangeTracker.Clear();

        var persistedMaterial =
            await Context.Materials
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal("Aluminio", persistedMaterial.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMaterials()
    {
        var materialOne =
            new Material("Acero");

        var materialTwo =
            new Material("Aluminio");

        Context.Materials.AddRange(
            materialOne,
            materialTwo);

        await Context.SaveChangesAsync();

        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.GetAllAsync();

        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            material => material.Name == "Acero");

        Assert.Contains(
            result,
            material => material.Name == "Aluminio");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoMaterialsExist()
    {
        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMaterials_WithPartialNameMatch()
    {
        var materialOne =
            new Material("Acero Inoxidable");

        var materialTwo =
            new Material("Acero Galvanizado");

        var materialThree =
            new Material("Aluminio");

        Context.Materials.AddRange(
            materialOne,
            materialTwo,
            materialThree);

        await Context.SaveChangesAsync();

        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.SearchByNameAsync("Acero");

        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            material => material.Name == "Acero inoxidable");

        Assert.Contains(
            result,
            material => material.Name == "Acero galvanizado");
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldBeCaseInsensitive()
    {
        var material =
            new Material("Acero Inoxidable");

        Context.Materials.Add(material);
        await Context.SaveChangesAsync();

        var repository =
            new MaterialRepository(Context);

        var result =
            await repository.SearchByNameAsync("ACERO");

        Assert.Single(result);

        Assert.Equal(
            "Acero inoxidable",
            result[0].Name);
    }
}