using PurchaseTicket.Application.Abstractions.Backups;

namespace PurchaseTicket.Infrastructure.Backups;

// Solo ve los archivos con el nombre de respaldo: el resto de la carpeta,
// como un ".partial" a medias o archivos del operador, no se lista ni se borra
public sealed class FileBackupStorage : IBackupStorage
{
    // Metodos publicos

    public bool FolderExists(string folder)
    {
        return !string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder);
    }

    public IReadOnlyList<BackupFile> List(string folder)
    {
        return new DirectoryInfo(folder)
            .EnumerateFiles(BackupFileNames.SearchPattern)
            .Where(file => file.Name.EndsWith(BackupFileNames.Extension, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => new BackupFile(file.FullName, file.LastWriteTimeUtc, file.Length))
            .ToList();
    }

    public void Delete(BackupFile file)
    {
        File.Delete(file.Path);
    }
}
