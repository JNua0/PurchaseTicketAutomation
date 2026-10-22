using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.PurchaseTickets.GetDetail;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Tests.UseCases.PurchaseTickets;

public class GetPurchaseTicketDetailTests
{
    private sealed class FakePurchaseTicketQueryRepository
        : IPurchaseTicketQueryRepository
    {
        public PurchaseTicketDetail? Detail { get; init; }

        public int? ReceivedId { get; private set; }

        public Task<PurchaseTicketDetail?> GetDetailAsync(int id)
        {
            ReceivedId = id;

            return Task.FromResult(Detail);
        }

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
    public async Task ExecuteAsync_ShouldReturnDetail()
    {
        // Arrange
        var detail = new PurchaseTicketDetail(
            Id: 5,
            TicketNumber: "5",
            Status: TicketStatus.Completed,
            WeighingType: WeighingType.Single,
            CheckInAt: new DateTime(2026, 10, 21, 15, 0, 0, DateTimeKind.Utc),
            DepartureAt: null,
            SupplierId: 3,
            SupplierName: "Proveedor Uno",
            MaterialId: 7,
            MaterialName: "Archivo",
            LicensePlate: null,
            Transporter: "Juan Perez",
            GrossWeight: null,
            TareWeight: null,
            NetWeight: 1237m,
            Discount: 3,
            DiscountWeight: 37.11m,
            NetWeightAfterDiscount: 1199.89m,
            PricePerKg: 3.33m,
            Amount: 3995.63m,
            UpdatedAt: null);

        var repository = new FakePurchaseTicketQueryRepository { Detail = detail };

        var useCase = new GetPurchaseTicketDetail(repository);

        // Act
        var result = await useCase.ExecuteAsync(5);

        // Assert
        Assert.Same(detail, result);
        Assert.Equal(5, repository.ReceivedId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenTicketDoesNotExist()
    {
        // Arrange
        var repository = new FakePurchaseTicketQueryRepository();

        var useCase = new GetPurchaseTicketDetail(repository);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => useCase.ExecuteAsync(99));

        // Assert
        Assert.Equal("No se encontró el ticket.", exception.Message);
    }
}