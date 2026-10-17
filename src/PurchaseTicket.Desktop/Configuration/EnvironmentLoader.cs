using DotNetEnv;
using System.IO;

namespace PurchaseTicket.Desktop.Configuration;

public static class EnvironmentLoader
{
    public static void Load()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var envPath = Path.Combine(
                directory.FullName,
                ".env");

            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                return;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("No se encontró el archivo .env.");
    }
}