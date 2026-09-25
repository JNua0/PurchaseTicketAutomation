using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Materials.Update;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Materials;

public class UpdateMaterialTests
{
    private sealed class FakeMaterialRepository
        : IMaterialRepository
    {
        public Material? MaterialToReturn { get; set; }
        public Material? UpdatedMaterial { get; private set; }
        public bool NameExists { get; set; }
        public string? CheckedName { get; private set; }
        public int? ExcludedMaterialId { get; private set; }

        public Task AddAsync(Material material)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeMaterialId = null)
        {
            CheckedName = name;
            ExcludedMaterialId = excludeMaterialId;

            return Task.FromResult(NameExists);
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
    public async Task ExecuteAsync_ShouldUpdateMaterial()
    {
        // Arrange
        var material = new Material("Acero");

        var repository = new FakeMaterialRepository
        {
            MaterialToReturn = material
        };

        var useCase = new UpdateMaterial(repository);

        var command = new UpdateMaterialCommand(
            1,
            "Acero inoxidable");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "Acero inoxidable",
            material.Name);

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

        var useCase = new UpdateMaterial(repository);

        var command = new UpdateMaterialCommand(
            1,
            "Acero inoxidable");

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.UpdatedMaterial);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenMaterialNameAlreadyExists()
    {
        // Arrange
        var material = new Material("Acero");

        var repository = new FakeMaterialRepository
        {
            MaterialToReturn = material,
            NameExists = true
        };

        var useCase = new UpdateMaterial(repository);

        var command = new UpdateMaterialCommand(
            1,
            "Cobre");

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Equal("Acero", material.Name);
        Assert.Null(repository.UpdatedMaterial);
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldCheckExistenceUsingNormalizedNameAndExcludeCurrentMaterial()
    {
        // Arrange
        var material = new Material("Acero");

        var repository = new FakeMaterialRepository
        {
            MaterialToReturn = material
        };

        var useCase = new UpdateMaterial(repository);

        var command = new UpdateMaterialCommand(
            1,
            "   ACERO INOXIDABLE   ");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "Acero inoxidable",
            repository.CheckedName);

        Assert.Equal(
            1,
            repository.ExcludedMaterialId);
    }
}