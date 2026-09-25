using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Materials.Deactivate;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Materials;

public class DeactivateMaterialTests
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
    public async Task ExecuteAsync_ShouldDeactivateMaterial()
    {
        // Arrange
        var material = new Material("Acero");

        var repository = new FakeMaterialRepository
        {
            MaterialToReturn = material
        };

        var useCase = new DeactivateMaterial(repository);

        // Act
        await useCase.ExecuteAsync(1);

        // Assert
        Assert.False(material.IsActive);

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

        var useCase = new DeactivateMaterial(repository);

        // Act
        async Task Act() => await useCase.ExecuteAsync(1);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.UpdatedMaterial);
    }
}