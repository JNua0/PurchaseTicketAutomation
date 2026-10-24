namespace PurchaseTicket.Application.Abstractions.Backups;

// Los archivos de respaldo de una carpeta; los demas archivos no se tocan
public interface IBackupStorage
{
    bool FolderExists(string folder);

    // Del mas reciente al mas antiguo
    IReadOnlyList<BackupFile> List(string folder);

    void Delete(BackupFile file);
}
