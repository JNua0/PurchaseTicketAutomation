using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Materials.Get;

namespace PurchaseTicket.Application.Tests.UseCases.Materials;

public class GetMaterialsTests
{
    private sealed class FakeMaterialQueryRepository : IMaterialQueryRepository
    {
        public int? ReceivedPage { get; private set; }
        public int? ReceivedPageSize { get; private set; }
        public string? ReceivedSearch { get; private set; }
        public bool? ReceivedIsActive { get; private set; }

        public PagedResult<MaterialListItem> ResultToReturn { get; set; }
            = new([], 1, 20, 0);

        public Task<PagedResult<MaterialListItem>> GetPagedAsync(
            int page,
            int pageSize,
            string? search = null,
            bool? isActive = null)
        {
            ReceivedPage = page;
            ReceivedPageSize = pageSize;
            ReceivedSearch = search;
            ReceivedIsActive = isActive;

            return Task.FromResult(ResultToReturn);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedMaterials()
    {
        // Arrange
        var repository = new FakeMaterialQueryRepository
        {
            ResultToReturn = new PagedResult<MaterialListItem>(
                [
                    new MaterialListItem(
                        1,
                        "Acero",
                        true),

                    new MaterialListItem(
                        2,
                        "Cobre",
                        false)
                ],
                1,
                20,
                2)
        };

        var useCase = new GetMaterials(repository);

        // Act
        var result = await useCase.ExecuteAsync(
            new GetMaterialsQuery());

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);

        Assert.Equal("Acero", result.Items[0].Name);
        Assert.True(result.Items[0].IsActive);

        Assert.Equal("Cobre", result.Items[1].Name);
        Assert.False(result.Items[1].IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassPaginationAndFiltersToRepository()
    {
        // Arrange
        var repository = new FakeMaterialQueryRepository();

        var useCase = new GetMaterials(repository);

        var query = new GetMaterialsQuery(
            Page: 2,
            PageSize: 10,
            Search: "acero",
            IsActive: true);

        // Act
        await useCase.ExecuteAsync(query);

        // Assert
        Assert.Equal(2, repository.ReceivedPage);
        Assert.Equal(10, repository.ReceivedPageSize);
        Assert.Equal("acero", repository.ReceivedSearch);
        Assert.True(repository.ReceivedIsActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldThrowWhenPageIsInvalid(
    int page)
    {
        // Arrange
        var repository = new FakeMaterialQueryRepository();

        var useCase = new GetMaterials(repository);

        var query = new GetMaterialsQuery(
            Page: page);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(query));

        // Assert
        Assert.Equal("Page", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ExecuteAsync_ShouldThrowWhenPageSizeIsInvalid(
    int pageSize)
    {
        // Arrange
        var repository = new FakeMaterialQueryRepository();

        var useCase = new GetMaterials(repository);

        var query = new GetMaterialsQuery(
            PageSize: pageSize);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(query));

        // Assert
        Assert.Equal("PageSize", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldTrimSearch()
    {
        // Arrange
        var repository = new FakeMaterialQueryRepository();

        var useCase = new GetMaterials(repository);

        var query = new GetMaterialsQuery(
            Search: "   Acero   ");

        // Act
        await useCase.ExecuteAsync(query);

        // Assert
        Assert.Equal("Acero", repository.ReceivedSearch);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldConvertWhitespaceSearchToNull()
    {
        // Arrange
        var repository = new FakeMaterialQueryRepository();

        var useCase = new GetMaterials(repository);

        var query = new GetMaterialsQuery(
            Search: "     ");

        // Act
        await useCase.ExecuteAsync(query);

        // Assert
        Assert.Null(repository.ReceivedSearch);
    }
}