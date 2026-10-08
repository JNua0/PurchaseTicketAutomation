using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Materials.GetActive;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Materials;

public class GetActiveMaterialsTests
{
    private sealed class FakeMaterialRepository
        : IMaterialRepository
    {
        public IReadOnlyList<Material> ActiveMaterialsToReturn { get; set; }
            = [];

        public Task<IReadOnlyList<Material>> GetActiveAsync()
        {
            return Task.FromResult(ActiveMaterialsToReturn);
        }

        public Task AddAsync(Material material)
            => throw new NotImplementedException();

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeMaterialId = null)
            => throw new NotImplementedException();

        public Task<Material?> GetByIdAsync(int id)
            => throw new NotImplementedException();

        public Task UpdateAsync(Material material)
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenNoActiveMaterialsExist()
    {
        // Arrange
        var repository = new FakeMaterialRepository();

        var useCase = new GetActiveMaterials(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnActiveMaterialsAsOptions()
    {
        // Arrange
        var repository = new FakeMaterialRepository
        {
            ActiveMaterialsToReturn =
            [
                new Material("Acero"),
            new Material("Cobre")
            ]
        };

        var useCase = new GetActiveMaterials(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("Acero", result[0].Name);
        Assert.Equal("Cobre", result[1].Name);
    }


}