using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Suppliers.Get;

namespace PurchaseTicket.Application.Tests.UseCases.Suppliers;

public class GetSuppliersTests
{
    private sealed class FakeSupplierQueryRepository : ISupplierQueryRepository
    {
        public PagedResult<SupplierListItem> ResultToReturn { get; set; }
            = new([], 1, 20, 0);

        public int? ReceivedPage { get; private set; }
        public int? ReceivedPageSize { get; private set; }
        public string? ReceivedSearch { get; private set; }
        public bool? ReceivedIsActive { get; private set; }

        public Task<PagedResult<SupplierListItem>> GetPagedAsync(
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
    public async Task ExecuteAsync_ShouldReturnPagedSuppliers()
    {
        // Arrange
        var repository = new FakeSupplierQueryRepository
        {
            ResultToReturn = new PagedResult<SupplierListItem>(
                [
                    new SupplierListItem(
                        1,
                        "Proveedor uno",
                        "5512345678",
                        true),

                    new SupplierListItem(
                        2,
                        "Proveedor dos",
                        null,
                        false)
                ],
                1,
                20,
                2)
        };

        var useCase = new GetSuppliers(repository);

        // Act
        var result = await useCase.ExecuteAsync(
            new GetSuppliersQuery());

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(2, result.TotalCount);

        Assert.Equal(
            "Proveedor uno",
            result.Items[0].Name);

        Assert.Equal(
            "5512345678",
            result.Items[0].PhoneNumber);

        Assert.True(result.Items[0].IsActive);

        Assert.Equal(
            "Proveedor dos",
            result.Items[1].Name);

        Assert.Null(result.Items[1].PhoneNumber);
        Assert.False(result.Items[1].IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassPaginationAndFiltersToRepository()
    {
        // Arrange
        var repository = new FakeSupplierQueryRepository();

        var useCase = new GetSuppliers(repository);

        var query = new GetSuppliersQuery(
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
        var repository = new FakeSupplierQueryRepository();

        var useCase = new GetSuppliers(repository);

        var query = new GetSuppliersQuery(
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
        var repository = new FakeSupplierQueryRepository();

        var useCase = new GetSuppliers(repository);

        var query = new GetSuppliersQuery(
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
        var repository = new FakeSupplierQueryRepository();

        var useCase = new GetSuppliers(repository);

        var query = new GetSuppliersQuery(
            Search: "   acero   ");

        // Act
        await useCase.ExecuteAsync(query);

        // Assert
        Assert.Equal(
            "acero",
            repository.ReceivedSearch);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldConvertWhitespaceSearchToNull()
    {
        // Arrange
        var repository = new FakeSupplierQueryRepository();

        var useCase = new GetSuppliers(repository);

        var query = new GetSuppliersQuery(
            Search: "     ");

        // Act
        await useCase.ExecuteAsync(query);

        // Assert
        Assert.Null(repository.ReceivedSearch);
    }
}