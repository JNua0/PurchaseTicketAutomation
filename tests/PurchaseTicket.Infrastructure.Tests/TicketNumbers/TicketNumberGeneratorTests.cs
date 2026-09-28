using PurchaseTicket.Application.Abstractions.TicketNumbers;
using PurchaseTicket.Infrastructure.TicketNumbers;

namespace PurchaseTicket.Infrastructure.Tests.TicketNumbers;

public class TicketNumberGeneratorTests : InfrastructureTestBase
{
    [Fact]
    public async Task GenerateAsync_ShouldReturnConsecutiveTicketNumbers()
    {
        var generator = new TicketNumberGenerator(Context);

        var firstTicketNumber = await generator.GenerateAsync();
        var secondTicketNumber = await generator.GenerateAsync();

        Assert.Equal(
            int.Parse(firstTicketNumber) + 1,
            int.Parse(secondTicketNumber));
    }

    [Fact]
    public async Task GenerateAsync_ShouldReturnNumericTicketNumber()
    {
        var generator = new TicketNumberGenerator(Context);

        var ticketNumber = await generator.GenerateAsync();

        Assert.True(long.TryParse(ticketNumber, out _));
    }
}