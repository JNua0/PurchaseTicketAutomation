using PurchaseTicket.Infrastructure.Weighing;

namespace PurchaseTicket.Infrastructure.Tests.Weighing;

public class DiniArgeoWeightParserTests
{
    [Fact]
    public void TryParseStableWeight_ShouldParseStableWeight()
    {
        // Arrange
        const string data = "ST,GS,    5980,kg\r";

        // Act
        var result =
            DiniArgeoWeightParser.TryParseStableWeight(
                data,
                out var weight);

        // Assert
        Assert.True(result);
        Assert.Equal(5980m, weight);
    }

    [Fact]
    public void TryParseStableWeight_ShouldParseZeroWeight()
    {
        // Arrange
        const string data = "ST,GS,       0,kg\r";

        // Act
        var result =
            DiniArgeoWeightParser.TryParseStableWeight(
                data,
                out var weight);

        // Assert
        Assert.True(result);
        Assert.Equal(0m, weight);
    }

    [Fact]
    public void TryParseStableWeight_ShouldParseNegativeWeight()
    {
        // Arrange
        const string data = "ST,GS,     -10,kg\r";

        // Act
        var result =
            DiniArgeoWeightParser.TryParseStableWeight(
                data,
                out var weight);

        // Assert
        Assert.True(result);
        Assert.Equal(-10m, weight);
    }

    [Fact]
    public void TryParseStableWeight_ShouldRejectUnstableWeight()
    {
        // Arrange
        const string data = "US,GS,    5980,kg\r";

        // Act
        var result =
            DiniArgeoWeightParser.TryParseStableWeight(
                data,
                out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryParseStableWeight_ShouldRejectNonKilogramUnit()
    {
        // Arrange
        const string data = "ST,GS,    5980,lb\r";

        // Act
        var result =
            DiniArgeoWeightParser.TryParseStableWeight(
                data,
                out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryParseStableWeight_ShouldRejectMalformedData()
    {
        // Arrange
        const string data = "invalid data";

        // Act
        var result =
            DiniArgeoWeightParser.TryParseStableWeight(
                data,
                out _);

        // Assert
        Assert.False(result);
    }
}