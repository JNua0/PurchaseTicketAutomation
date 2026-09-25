using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Materials.Create;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Application.Tests.UseCases.Materials;

public class CreateMaterialTests
{
    private sealed class FakeMaterialRepository
        : IMaterialRepository
    {
        public Material? AddedMaterial { get; private set; }
        public bool NameExists { get; set; }
        public string? CheckedName { get; private set; }

        public Task AddAsync(Material material)
        {
            AddedMaterial = material;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            int? excludeMaterialId = null)
        {
            CheckedName = name;

            return Task.FromResult(NameExists);
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
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Material>> SearchByNameAsync(
    string name)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateAndStoreMaterial()
    {
        // Arrange
        var repository = new FakeMaterialRepository();

        var useCase = new CreateMaterial(repository);

        var command = new CreateMaterialCommand(
            "Acero inoxidable");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.NotNull(repository.AddedMaterial);

        Assert.Equal(
            "Acero inoxidable",
            repository.AddedMaterial.Name);

        Assert.True(repository.AddedMaterial.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotStoreMaterialWhenDataIsInvalid()
    {
        // Arrange
        var repository = new FakeMaterialRepository();

        var useCase = new CreateMaterial(repository);

        var command = new CreateMaterialCommand("   ");

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Null(repository.AddedMaterial);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenMaterialNameAlreadyExists()
    {
        // Arrange
        var repository = new FakeMaterialRepository
        {
            NameExists = true
        };

        var useCase = new CreateMaterial(repository);

        var command = new CreateMaterialCommand(
            "Acero inoxidable");

        // Act
        async Task Act() => await useCase.ExecuteAsync(command);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);

        Assert.Null(repository.AddedMaterial);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCheckExistenceUsingNormalizedName()
    {
        // Arrange
        var repository = new FakeMaterialRepository();

        var useCase = new CreateMaterial(repository);

        var command = new CreateMaterialCommand(
            "   ACERO INOXIDABLE   ");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        Assert.Equal(
            "Acero inoxidable",
            repository.CheckedName);
    }
}