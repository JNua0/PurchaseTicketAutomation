using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.UseCases.Reports.Purchases;

namespace PurchaseTicket.Application.Tests.UseCases.Reports;

public class GetPurchaseReportTests
{
    private sealed class FakeReportQueryRepository : IReportQueryRepository
    {
        public (DateTime From, DateTime To)? Requested { get; private set; }

        public Task<IReadOnlyList<ReportTicket>> GetTicketsAsync(DateTime fromUtc, DateTime toUtc)
        {
            Requested = (fromUtc, toUtc);

            return Task.FromResult<IReadOnlyList<ReportTicket>>([]);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldQueryTheRequestedPeriod()
    {
        // Arrange
        var repository = new FakeReportQueryRepository();
        var from = new DateTime(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 10, 24, 6, 0, 0, DateTimeKind.Utc);

        // Act
        var report = await new GetPurchaseReport(repository).ExecuteAsync(from, to);

        // Assert
        Assert.Equal((from, to), repository.Requested);
        Assert.Equal(from, report.FromUtc);
        Assert.Equal(to, report.ToUtc);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenEndIsNotAfterStart()
    {
        // Arrange
        var repository = new FakeReportQueryRepository();
        var date = new DateTime(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc);

        // Act / Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => new GetPurchaseReport(repository).ExecuteAsync(date, date));

        Assert.Null(repository.Requested);
    }
}
