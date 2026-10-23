using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.GetFilterOptions;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class GetTicketFilterOptionsTests
{
    private sealed class FakePurchaseTicketQueryRepository
        : IPurchaseTicketQueryRepository
    {
        public IReadOnlyList<CatalogOption> Suppliers { get; init; } = [];

        public IReadOnlyList<CatalogOption> Materials { get; init; } = [];

        public Task<IReadOnlyList<CatalogOption>> GetSuppliersInUseAsync()
            => Task.FromResult(Suppliers);

        public Task<IReadOnlyList<CatalogOption>> GetMaterialsInUseAsync()
            => Task.FromResult(Materials);

        public Task<DayWeighingTotals> GetDayTotalsAsync(DateTime from, DateTime to)
            => throw new NotImplementedException();

        public Task<PurchaseTicketDetail?> GetDetailAsync(int id)
            => throw new NotImplementedException();

        public Task<PagedResult<PurchaseTicketListItem>> GetPagedAsync(
            int page,
            int pageSize,
            string? search = null,
            TicketStatus? status = null,
            WeighingType? weighingType = null,
            DateTime? weighedFrom = null,
            DateTime? weighedTo = null,
            bool oldestFirst = false,
            int? supplierId = null,
            int? materialId = null)
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuppliersAndMaterialsInUse()
    {
        // Arrange
        var repository = new FakePurchaseTicketQueryRepository
        {
            Suppliers =
            [
                new CatalogOption(1, "Aceros Del Norte", IsActive: true),
                new CatalogOption(2, "Proveedor Viejo", IsActive: false)
            ],
            Materials =
            [
                new CatalogOption(5, "Cartón", IsActive: true)
            ]
        };

        var useCase = new GetTicketFilterOptions(repository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.Same(repository.Suppliers, result.Suppliers);
        Assert.Same(repository.Materials, result.Materials);
    }
}