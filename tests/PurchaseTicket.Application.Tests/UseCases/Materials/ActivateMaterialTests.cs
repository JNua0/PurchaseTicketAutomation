using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Materials.Activate;
using PurchaseTicket.Application.UseCases.Materials.Deactivate;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Materials;

public class ActivateMaterialTests
{
    private sealed class FakeMaterialRepository
        : IMaterialRepository
    {
        public Material? MaterialToReturn { get; set; }

        public Material? UpdatedMaterial { get; private set; }

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
            return Task.FromResult(MaterialToReturn);
        }

        public Task UpdateAsync(Material material)
        {
            UpdatedMaterial = material;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Material>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Material>> SearchByNameAsync(
    string name)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldActivateMaterial()
    {
        // Arrange
        var material = new Material("Acero");

        material.Deactivate();

        var repository = new FakeMaterialRepository
        {
            MaterialToReturn = material
        };

        var useCase = new ActivateMaterial(repository);

        // Act
        await useCase.ExecuteAsync(1);

        // Assert
        Assert.True(material.IsActive);

        Assert.Same(
            material,
            repository.UpdatedMaterial);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenMaterialDoesNotExist()
    {
        // Arrange
        var repository = new FakeMaterialRepository
        {
            MaterialToReturn = null
        };

        var useCase = new ActivateMaterial(repository);

        // Act
        async Task Act() => await useCase.ExecuteAsync(1);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.UpdatedMaterial);
    }
}