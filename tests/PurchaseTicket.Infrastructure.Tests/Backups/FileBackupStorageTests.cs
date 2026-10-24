using PurchaseTicket.Infrastructure.Backups;

namespace PurchaseTicket.Infrastructure.Tests.Backups;

// Usa una carpeta temporal propia que se borra al terminar
public sealed class FileBackupStorageTests : IDisposable
{
    private readonly string _folder =
        Path.Combine(Path.GetTempPath(), "respaldos-" + Guid.NewGuid().ToString("N"));

    public FileBackupStorageTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
    }

    private string CreateFile(string name, DateTime lastWriteUtc, int size = 10)
    {
        string path = Path.Combine(_folder, name);

        File.WriteAllBytes(path, new byte[size]);
        File.SetLastWriteTimeUtc(path, lastWriteUtc);

        return path;
    }

    [Fact]
    public void List_ShouldReturnOnlyBackupsNewestFirst()
    {
        // Arrange
        string older = CreateFile("ecopapel_2026-10-21_080000.dump", new DateTime(2026, 10, 21, 14, 0, 0, DateTimeKind.Utc));
        string newer = CreateFile("ecopapel_2026-10-22_080000.dump", new DateTime(2026, 10, 22, 14, 0, 0, DateTimeKind.Utc), 25);
        CreateFile("ecopapel_2026-10-23_080000.dump.partial", new DateTime(2026, 10, 23, 14, 0, 0, DateTimeKind.Utc));
        CreateFile("notas.txt", new DateTime(2026, 10, 23, 14, 0, 0, DateTimeKind.Utc));

        // Act
        var backups = new FileBackupStorage().List(_folder);

        // Assert
        Assert.Equal(new[] { newer, older }, backups.Select(backup => backup.Path).ToArray());
        Assert.Equal(25, backups[0].SizeBytes);
    }

    [Fact]
    public void Delete_ShouldRemoveTheFile()
    {
        // Arrange
        var storage = new FileBackupStorage();
        CreateFile("ecopapel_2026-10-21_080000.dump", DateTime.UtcNow);
        var backup = Assert.Single(storage.List(_folder));

        // Act
        storage.Delete(backup);

        // Assert
        Assert.Empty(storage.List(_folder));
    }

    [Fact]
    public void FolderExists_ShouldBeFalseForMissingOrEmptyPath()
    {
        var storage = new FileBackupStorage();

        Assert.True(storage.FolderExists(_folder));
        Assert.False(storage.FolderExists(Path.Combine(_folder, "no-existe")));
        Assert.False(storage.FolderExists(string.Empty));
    }
}
