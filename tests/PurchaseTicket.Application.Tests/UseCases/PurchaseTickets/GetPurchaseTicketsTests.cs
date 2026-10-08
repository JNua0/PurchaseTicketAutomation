using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class GetPurchaseTicketsTests
{
    private sealed class FakePurchaseTicketQueryRepository
        : IPurchaseTicketQueryRepository
    {
        public PagedResult<PurchaseTicketListItem> Result { get; set; } =
            new(
                [],
                Page: 1,
                PageSize: 20,
                TotalCount: 0);

        public int? ReceivedPage { get; private set; }
        public int? ReceivedPageSize { get; private set; }
        public string? ReceivedSearch { get; private set; }
        public TicketStatus? ReceivedStatus { get; private set; }
        public WeighingType? ReceivedWeighingType { get; private set; }

        public Task<PagedResult<PurchaseTicketListItem>> GetPagedAsync(
            int page,
            int pageSize,
            string? search = null,
            TicketStatus? status = null,
            WeighingType? weighingType = null)
        {
            ReceivedPage = page;
            ReceivedPageSize = pageSize;
            ReceivedSearch = search;
            ReceivedStatus = status;
            ReceivedWeighingType = weighingType;

            return Task.FromResult(Result);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedPurchaseTickets()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketQueryRepository
            {
                Result = new PagedResult<PurchaseTicketListItem>(
                    [
                        new PurchaseTicketListItem(
                            TicketNumber: "T-000001",
                            CheckInAt: new DateTime(
                                2026, 10, 6, 8, 30, 0),
                            SupplierName: "Proveedor Uno",
                            MaterialName: "Acero",
                            LicensePlate: "ABC123",
                            Transporter: "Juan Perez",
                            FinalWeight: 14250m,
                            Amount: 28500m,
                            WeighingType: WeighingType.Conventional,
                            Status: TicketStatus.Completed)
                    ],
                    Page: 1,
                    PageSize: 20,
                    TotalCount: 1)
            };

        var useCase =
            new GetPurchaseTickets(repository);

        var query =
            new GetPurchaseTicketsQuery();

        // Act
        var result =
            await useCase.ExecuteAsync(query);

        // Assert
        Assert.Single(result.Items);

        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);

        var item = result.Items[0];

        Assert.IsType<PurchaseTicketDto>(item);

        Assert.Equal(
            "T-000001",
            item.TicketNumber);

        Assert.Equal(
            new DateTime(2026, 10, 6, 8, 30, 0),
            item.CheckInAt);

        Assert.Equal(
            "Proveedor Uno",
            item.SupplierName);

        Assert.Equal(
            "Acero",
            item.MaterialName);

        Assert.Equal(
            "ABC123",
            item.LicensePlate);

        Assert.Equal(
            "Juan Perez",
            item.Transporter);

        Assert.Equal(
            14250m,
            item.FinalWeight);

        Assert.Equal(
            28500m,
            item.Amount);

        Assert.Equal(
            WeighingType.Conventional,
            item.WeighingType);

        Assert.Equal(
            TicketStatus.Completed,
            item.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassPaginationAndFiltersToRepository()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketQueryRepository();

        var useCase =
            new GetPurchaseTickets(repository);

        var query =
            new GetPurchaseTicketsQuery(
                Page: 2,
                PageSize: 10,
                Search: "T-000123",
                Status: TicketStatus.Completed,
                WeighingType: WeighingType.Conventional);

        // Act
        await useCase.ExecuteAsync(query);

        // Assert
        Assert.Equal(
            2,
            repository.ReceivedPage);

        Assert.Equal(
            10,
            repository.ReceivedPageSize);

        Assert.Equal(
            "T-000123",
            repository.ReceivedSearch);

        Assert.Equal(
            TicketStatus.Completed,
            repository.ReceivedStatus);

        Assert.Equal(
            WeighingType.Conventional,
            repository.ReceivedWeighingType);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyPageWhenThereAreNoPurchaseTickets()
    {
        // Arrange
        var repository =
            new FakePurchaseTicketQueryRepository
            {
                Result = new PagedResult<PurchaseTicketListItem>(
                    [],
                    Page: 1,
                    PageSize: 20,
                    TotalCount: 0)
            };

        var useCase =
            new GetPurchaseTickets(repository);

        var query =
            new GetPurchaseTicketsQuery();

        // Act
        var result =
            await useCase.ExecuteAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);

        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldThrowWhenPageIsInvalid(
    int page)
    {
        // Arrange
        var repository =
            new FakePurchaseTicketQueryRepository();

        var useCase =
            new GetPurchaseTickets(repository);

        var query =
            new GetPurchaseTicketsQuery(
                Page: page);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(query);

        // Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal(
            "Page must be greater than zero.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ExecuteAsync_ShouldThrowWhenPageSizeIsInvalid(
        int pageSize)
    {
        // Arrange
        var repository =
            new FakePurchaseTicketQueryRepository();

        var useCase =
            new GetPurchaseTickets(repository);

        var query =
            new GetPurchaseTicketsQuery(
                PageSize: pageSize);

        // Act
        async Task Act() =>
            await useCase.ExecuteAsync(query);

        // Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal(
            "Page size must be between 1 and 100.",
            exception.Message);
    }


}