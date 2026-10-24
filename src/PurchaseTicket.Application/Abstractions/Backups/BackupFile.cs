namespace PurchaseTicket.Application.Abstractions.Backups;

// Un archivo de respaldo en la carpeta; la fecha va en UTC
public sealed record BackupFile(
    string Path,
    DateTime CreatedAtUtc,
    long SizeBytes);
