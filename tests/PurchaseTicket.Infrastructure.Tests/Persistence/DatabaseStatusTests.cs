using PurchaseTicket.Infrastructure.Persistence;

namespace PurchaseTicket.Infrastructure.Tests.Persistence;

// La base de pruebas ya esta migrada por InfrastructureTestBase
public class DatabaseStatusTests : InfrastructureTestBase
{
    [Fact]
    public async Task CanConnectAsync_ShouldBeTrueForTheTestDatabase()
    {
        var status = new DatabaseStatus(Context);

        Assert.True(await status.CanConnectAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task GetPendingMigrationsAsync_ShouldBeEmptyAfterMigrating()
    {
        var status = new DatabaseStatus(Context);

        Assert.Empty(await status.GetPendingMigrationsAsync());
        Assert.True(await status.CountAppliedMigrationsAsync() > 0);
    }
}
