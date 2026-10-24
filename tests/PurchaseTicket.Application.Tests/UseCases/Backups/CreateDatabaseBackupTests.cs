using Microsoft.Extensions.Logging.Abstractions;
using PurchaseTicket.Application.Abstractions.Backups;
using PurchaseTicket.Application.UseCases.Backups;
using PurchaseTicket.Application.UseCases.Backups.Create;

namespace PurchaseTicket.Application.Tests.UseCases.Backups;

public class CreateDatabaseBackupTests
{
    private static readonly DateTime Now = new(2026, 10, 23, 14, 0, 0, DateTimeKind.Utc);

    private sealed class FakeBackupService : IDatabaseBackupService
    {
        public int CreateCount { get; private set; }

        public BackupFile Result { get; init; } = new("nuevo.dump", Now, 2048);

        public Exception? Error { get; init; }

        public Task<BackupFile> CreateAsync(string folder, CancellationToken cancellationToken = default)
        {
            CreateCount++;

            return Error is null
                ? Task.FromResult(Result)
                : Task.FromException<BackupFile>(Error);
        }
    }

    private sealed class FakeBackupStorage : IBackupStorage
    {
        public bool Exists { get; init; } = true;

        public List<BackupFile> Files { get; } = [];

        public List<BackupFile> Deleted { get; } = [];

        public BackupFile? FailOnDelete { get; init; }

        public bool FolderExists(string folder) => Exists;

        public IReadOnlyList<BackupFile> List(string folder)
        {
            return Files.OrderByDescending(file => file.CreatedAtUtc).ToList();
        }

        public void Delete(BackupFile file)
        {
            if (file == FailOnDelete)
                throw new IOException("El archivo esta en uso.");

            Deleted.Add(file);
            Files.Remove(file);
        }
    }

    // count respaldos, uno por dia hacia atras
    private static List<BackupFile> DailyBackups(int count)
    {
        return Enumerable.Range(0, count)
            .Select(day => new BackupFile($"r{day}.dump", Now.AddDays(-day), 1024))
            .ToList();
    }

    private static CreateDatabaseBackup CreateUseCase(
        IDatabaseBackupService service,
        IBackupStorage storage)
    {
        return new CreateDatabaseBackup(
            service,
            storage,
            NullLogger<CreateDatabaseBackup>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenFolderDoesNotExist()
    {
        // Arrange
        var service = new FakeBackupService();
        var storage = new FakeBackupStorage { Exists = false };

        // Act
        var exception = await Assert.ThrowsAsync<BackupFailedException>(
            () => CreateUseCase(service, storage).ExecuteAsync(@"E:\Respaldos"));

        // Assert
        Assert.Equal(
            @"No se encontró la carpeta E:\Respaldos. Conecta la unidad o elige otra carpeta.",
            exception.Message);

        Assert.Equal(0, service.CreateCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnCreatedBackup()
    {
        // Arrange
        var service = new FakeBackupService();
        var storage = new FakeBackupStorage();

        // Act
        var backup = await CreateUseCase(service, storage).ExecuteAsync("C:\\Respaldos");

        // Assert
        Assert.Same(service.Result, backup);
        Assert.Equal(1, service.CreateCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepOnlyTheNewestBackups()
    {
        // Arrange
        var service = new FakeBackupService();
        var storage = new FakeBackupStorage();
        storage.Files.AddRange(DailyBackups(BackupSchedule.KeepCount + 3));

        // Act
        await CreateUseCase(service, storage).ExecuteAsync("C:\\Respaldos");

        // Assert: se borran los 3 mas antiguos
        Assert.Equal(
            new[] { "r32.dump", "r31.dump", "r30.dump" },
            storage.Deleted.Select(file => file.Path).OrderByDescending(path => path).ToArray());

        Assert.Equal(BackupSchedule.KeepCount, storage.Files.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotDeleteWhenBackupFails()
    {
        // Arrange
        var service = new FakeBackupService { Error = new BackupFailedException("Docker Desktop no está abierto.") };
        var storage = new FakeBackupStorage();
        storage.Files.AddRange(DailyBackups(BackupSchedule.KeepCount + 3));

        // Act
        await Assert.ThrowsAsync<BackupFailedException>(
            () => CreateUseCase(service, storage).ExecuteAsync("C:\\Respaldos"));

        // Assert
        Assert.Empty(storage.Deleted);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepGoingWhenAnOldBackupCannotBeDeleted()
    {
        // Arrange
        var service = new FakeBackupService();
        var files = DailyBackups(BackupSchedule.KeepCount + 2);
        var storage = new FakeBackupStorage { FailOnDelete = files[^1] };
        storage.Files.AddRange(files);

        // Act
        var backup = await CreateUseCase(service, storage).ExecuteAsync("C:\\Respaldos");

        // Assert
        Assert.Same(service.Result, backup);
        Assert.Equal("r30.dump", Assert.Single(storage.Deleted).Path);
    }
}
