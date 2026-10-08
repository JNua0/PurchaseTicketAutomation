using PurchaseTicket.Application.Abstractions.Weighing;
using PurchaseTicket.Application.UseCases.Weighing.ReadWeight;

namespace PurchaseTicket.Application.Tests.UseCases.Weighing.ReadWeight;

public class ReadWeightTests
{
    private sealed class FakeWeightReader : IWeightReader
    {
        private readonly decimal _weight;

        public FakeWeightReader(decimal weight)
        {
            _weight = weight;
        }

        public Task<decimal> ReadStableWeightAsync()
        {
            return Task.FromResult(_weight);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnWeight()
    {
        // Arrange
        var weightReader =
            new FakeWeightReader(25000m);

        var useCase =
            new ReadWeightUseCase(weightReader);

        // Act
        var weight =
            await useCase.ExecuteAsync();

        // Assert
        Assert.Equal(25000m, weight);
    }
}