using PurchaseTicket.Application.Abstractions.Backups;
using PurchaseTicket.Application.UseCases.Backups;

namespace PurchaseTicket.Application.Tests.UseCases.Backups;

public class BackupScheduleTests
{
    private static readonly DateTime Now = new(2026, 10, 23, 14, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsDue_ShouldBeTrueWhenThereAreNoBackups()
    {
        Assert.True(BackupSchedule.IsDue(null, Now));
    }

    [Fact]
    public void IsDue_ShouldBeFalseWhenLatestIsLessThanADayOld()
    {
        // Arrange
        var latest = new BackupFile("a.dump", Now.AddHours(-23), 100);

        // Act / Assert
        Assert.False(BackupSchedule.IsDue(latest, Now));
    }

    [Fact]
    public void IsDue_ShouldBeTrueWhenLatestIsADayOld()
    {
        // Arrange
        var latest = new BackupFile("a.dump", Now.AddHours(-24), 100);

        // Act / Assert
        Assert.True(BackupSchedule.IsDue(latest, Now));
    }
}
