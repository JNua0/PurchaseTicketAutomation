using PurchaseTicket.Application.Abstractions.Backups;

namespace PurchaseTicket.Application.UseCases.Backups.GetLatest;

// El respaldo mas reciente de la carpeta; null si no hay o la carpeta no existe
public class GetLatestBackup
{
    // Campos

    private readonly IBackupStorage _storage;

    // Constructor

    public GetLatestBackup(IBackupStorage storage)
    {
        _storage = storage;
    }

    // Metodos publicos

    public BackupFile? Execute(string folder)
    {
        return _storage.FolderExists(folder)
            ? _storage.List(folder).FirstOrDefault()
            : null;
    }
}
