using System.Globalization;

namespace PurchaseTicket.Infrastructure.Backups;

// Nombre de los archivos de respaldo: "ecopapel_2026-10-23_080215.dump",
// con la hora local para que se reconozcan al abrir la carpeta
public static class BackupFileNames
{
    // Campos

    public const string Prefix = "ecopapel_";

    public const string Extension = ".dump";

    public const string SearchPattern = Prefix + "*" + Extension;

    // Metodos publicos

    public static string For(DateTime localTime)
    {
        return Prefix + localTime.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture) + Extension;
    }
}
