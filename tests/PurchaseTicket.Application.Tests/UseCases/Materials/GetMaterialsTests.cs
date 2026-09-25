using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Materials.Get;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Materials;

public class GetMaterialsTests
{
    private sealed class FakeMaterialRepository
        : IMaterialRepository
    {
        public string? SearchName { get; private set; }
        public IReadOnlyList<Material> MaterialsToReturn { get; set; }
            = [];

        public Task AddAsync(Material material)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeMaterialId = null)
        {
            throw new NotImplementedException();
        }

        public Task<Material?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Material material)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Material>> GetAllAsync()
        {
            return Task.FromResult(MaterialsToReturn);
        }

        public Task<IReadOnlyList<Material>> SearchByNameAsync(
            string name)
        {
            SearchName = name;

            IReadOnlyList<Material> result = MaterialsToReturn
                .Where(material =>
                    material.Name.Contains(
                        name,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(result);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllRegisteredMaterials()
    {
        // Arrange
        var activeMaterial = new Material("Acero");

        var inactiveMaterial = new Material("Cobre");
        inactiveMaterial.Deactivate();

        var repository = new FakeMaterialRepository
        {
            MaterialsToReturn =
            [
                activeMaterial,
                inactiveMaterial
            ]
        };

        var useCase = new GetMaterials(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Contains(activeMaterial, result);
        Assert.Contains(inactiveMaterial, result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyListWhenNoMaterialsExist()
    {
        // Arrange
        var repository = new FakeMaterialRepository
        {
            MaterialsToReturn = []
        };

        var useCase = new GetMaterials(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnMaterialsMatchingName()
    {
        // Arrange
        var steel = new Material("Acero inoxidable");
        var carbonSteel = new Material("Acero al carbono");
        var copper = new Material("Cobre");

        var repository = new FakeMaterialRepository
        {
            MaterialsToReturn =
            [
                steel,
            carbonSteel,
            copper
            ]
        };

        var useCase = new GetMaterials(repository);

        // Act
        var result = await useCase.ExecuteAsync("ACERO");

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Contains(steel, result);
        Assert.Contains(carbonSteel, result);
        Assert.DoesNotContain(copper, result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyListWhenNoMaterialsMatch()
    {
        // Arrange
        var repository = new FakeMaterialRepository
        {
            MaterialsToReturn =
            [
                new Material("Acero"),
            new Material("Cobre")
            ]
        };

        var useCase = new GetMaterials(repository);

        // Act
        var result = await useCase.ExecuteAsync("Aluminio");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldTrimSearchTermBeforeSearching()
    {
        // Arrange
        var repository = new FakeMaterialRepository
        {
            MaterialsToReturn =
            [
                new Material("Acero")
            ]
        };

        var useCase = new GetMaterials(repository);

        // Act
        await useCase.ExecuteAsync("   Acero   ");

        // Assert
        Assert.Equal("Acero", repository.SearchName);
    }
}