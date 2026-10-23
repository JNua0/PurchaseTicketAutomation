using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Home.GetSummary;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Tests.UseCases.Home;

public class GetHomeSummaryTests
{
    private static readonly DateTime DayStart = new(2026, 10, 22, 6, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime DayEnd = DayStart.AddDays(1);

    private sealed class FakePurchaseTicketQueryRepository
        : IPurchaseTicketQueryRepository
    {
        public Dictionary<TicketStatus, PagedResult<PurchaseTicketListItem>> ByStatus { get; } = new();

        public List<(TicketStatus? Status, int PageSize, bool OldestFirst)> PagedCalls { get; } = [];

        public DayWeighingTotals Totals { get; init; } = new(0, 0, 0, 0m);

        public (DateTime From, DateTime To)? ReceivedRange { get; private set; }

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
        {
            PagedCalls.Add((status, pageSize, oldestFirst));

            return Task.FromResult(
                status is TicketStatus value && ByStatus.TryGetValue(value, out var result)
                    ? result
                    : new PagedResult<PurchaseTicketListItem>([], page, pageSize, 0));
        }

        public Task<DayWeighingTotals> GetDayTotalsAsync(DateTime from, DateTime to)
        {
            ReceivedRange = (from, to);

            return Task.FromResult(Totals);
        }

        public Task<PurchaseTicketDetail?> GetDetailAsync(int id)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<CatalogOption>> GetSuppliersInUseAsync()
            => throw new NotImplementedException();

        public Task<IReadOnlyList<CatalogOption>> GetMaterialsInUseAsync()
            => throw new NotImplementedException();
    }

    private static PurchaseTicketListItem Item(int id, TicketStatus status)
    {
        return new PurchaseTicketListItem(
            id,
            TicketNumber: id.ToString(),
            CheckInAt: DayStart.AddHours(id),
            DepartureAt: null,
            SupplierId: 1,
            MaterialId: 1,
            SupplierName: "Aceros Del Norte",
            MaterialName: "Acero",
            LicensePlate: "ABC123",
            Transporter: "Juan Perez",
            GrossWeight: 25000m,
            NetWeight: null,
            FinalWeight: null,
            Amount: null,
            WeighingType: WeighingType.Conventional,
            Status: status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOldestFiveOfEachStatusWithTheirTotals()
    {
        // Arrange
        var repository = new FakePurchaseTicketQueryRepository
        {
            Totals = new DayWeighingTotals(8, 5, 3, 23580m)
        };

        repository.ByStatus[TicketStatus.WeighingPending] =
            new([Item(14, TicketStatus.WeighingPending)], 1, 5, TotalCount: 3);

        repository.ByStatus[TicketStatus.AmountPending] =
            new([Item(8, TicketStatus.AmountPending), Item(10, TicketStatus.AmountPending)], 1, 5, TotalCount: 7);

        var useCase = new GetHomeSummary(repository);

        // Act
        var summary = await useCase.ExecuteAsync(new GetHomeSummaryQuery(DayStart, DayEnd));

        // Assert
        Assert.Equal(3, summary.InPlantCount);
        Assert.Equal("14", Assert.Single(summary.InPlant).TicketNumber);

        Assert.Equal(7, summary.AmountPendingCount);
        Assert.Equal(new[] { "8", "10" }, summary.AmountPending.Select(ticket => ticket.TicketNumber));

        Assert.Equal(repository.Totals, summary.Today);
        Assert.Equal((DayStart, DayEnd), repository.ReceivedRange);

        // Cada lista: los mas antiguos primero, hasta ListSize
        Assert.Contains((TicketStatus.WeighingPending, GetHomeSummary.ListSize, true), repository.PagedCalls);
        Assert.Contains((TicketStatus.AmountPending, GetHomeSummary.ListSize, true), repository.PagedCalls);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenDayStartIsNotBeforeDayEnd()
    {
        // Arrange
        var useCase = new GetHomeSummary(new FakePurchaseTicketQueryRepository());

        // Act
        Task Act() => useCase.ExecuteAsync(new GetHomeSummaryQuery(DayEnd, DayStart));

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal("El inicio del día debe ser anterior a su fin.", exception.Message);
    }
}
