using PurchaseTicket.Application.Abstractions.Backups;
using PurchaseTicket.Application.UseCases.Backups.GetLatest;

namespace PurchaseTicket.Application.Tests.UseCases.Backups;

public class GetLatestBackupTests
{
    private sealed class FakeBackupStorage : IBackupStorage
    {
        public bool Exists { get; init; } = true;

        public IReadOnlyList<BackupFile> Files { get; init; } = [];

        public bool FolderExists(string folder) => Exists;

        public IReadOnlyList<BackupFile> List(string folder) => Files;

        public void Delete(BackupFile file)
            => throw new NotImplementedException();
    }

    [Fact]
    public void Execute_ShouldReturnFirstListedBackup()
    {
        // Arrange
        var newest = new BackupFile("b.dump", new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc), 10);
        var oldest = new BackupFile("a.dump", new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc), 10);

        var storage = new FakeBackupStorage { Files = [newest, oldest] };

        // Act
        var latest = new GetLatestBackup(storage).Execute("C:\\Respaldos");

        // Assert
        Assert.Same(newest, latest);
    }

    [Fact]
    public void Execute_ShouldReturnNullWhenFolderDoesNotExist()
    {
        // Arrange
        var storage = new FakeBackupStorage { Exists = false };

        // Act / Assert
        Assert.Null(new GetLatestBackup(storage).Execute(@"E:\Respaldos"));
    }
}
