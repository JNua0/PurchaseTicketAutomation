using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Backups;

namespace PurchaseTicket.Application.UseCases.Backups.Create;

// Respalda la base de datos en la carpeta elegida y borra los respaldos
// que pasan de los ultimos 30. Lo usan el respaldo diario y el boton
// Respaldar ahora
public class CreateDatabaseBackup
{
    // Campos

    private readonly IDatabaseBackupService _backupService;
    private readonly IBackupStorage _storage;
    private readonly ILogger<CreateDatabaseBackup> _logger;

    // Constructor

    public CreateDatabaseBackup(
        IDatabaseBackupService backupService,
        IBackupStorage storage,
        ILogger<CreateDatabaseBackup> logger)
    {
        _backupService = backupService;
        _storage = storage;
        _logger = logger;
    }

    // Metodos publicos

    public async Task<BackupFile> ExecuteAsync(
        string folder,
        CancellationToken cancellationToken = default)
    {
        // Una USB desconectada: se avisa en lugar de crear la carpeta en otro lado
        if (!_storage.FolderExists(folder))
        {
            throw new BackupFailedException(
                $"No se encontró la carpeta {folder}. Conecta la unidad o elige otra carpeta.");
        }

        var backup = await _backupService.CreateAsync(folder, cancellationToken);

        DeleteOldBackups(folder);

        return backup;
    }

    // Metodos privados

    // El respaldo ya se creo: si no se puede borrar uno viejo solo se registra
    private void DeleteOldBackups(string folder)
    {
        foreach (var old in _storage.List(folder).Skip(BackupSchedule.KeepCount))
        {
            try
            {
                _storage.Delete(old);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No fue posible borrar el respaldo antiguo {Path}.", old.Path);
            }
        }
    }
}
