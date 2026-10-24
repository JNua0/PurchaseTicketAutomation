using PurchaseTicket.Infrastructure.Backups;

namespace PurchaseTicket.Infrastructure.Tests.Backups;

public class BackupFileNamesTests
{
    [Fact]
    public void For_ShouldUseLocalDateAndTime()
    {
        string name = BackupFileNames.For(new DateTime(2026, 10, 23, 8, 2, 15));

        Assert.Equal("ecopapel_2026-10-23_080215.dump", name);
    }
}
