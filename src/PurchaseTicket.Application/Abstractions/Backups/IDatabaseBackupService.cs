namespace PurchaseTicket.Application.Abstractions.Backups;

public interface IDatabaseBackupService
{
    // Respalda toda la base de datos en un archivo nuevo dentro de la carpeta.
    // Si falla lanza BackupFailedException y no deja un archivo a medias
    Task<BackupFile> CreateAsync(string folder, CancellationToken cancellationToken = default);
}
