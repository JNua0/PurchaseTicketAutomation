using Microsoft.Extensions.Configuration;
using Npgsql;
using PurchaseTicket.Infrastructure.Backups;

namespace PurchaseTicket.Desktop.Configuration;

public static class DatabaseConfiguration
{
    public static string GetConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = GetRequiredVariable("POSTGRES_HOST"),
            Database = GetRequiredVariable("POSTGRES_DB"),
            Username = GetRequiredVariable("POSTGRES_USER"),
            Password = GetRequiredVariable("POSTGRES_PASSWORD"),
            Port = int.Parse(
                GetRequiredVariable("POSTGRES_PORT"))
        };

        return builder.ConnectionString;
    }

    // Mismas credenciales que la conexion; el contenedor y la ruta de docker
    // se pueden cambiar en la seccion Backup de la configuracion
    public static DatabaseBackupOptions GetBackupOptions(IConfiguration configuration)
    {
        var defaults = new DatabaseBackupOptions();

        return new DatabaseBackupOptions
        {
            DockerPath = configuration["Backup:DockerPath"] ?? defaults.DockerPath,
            ContainerName = configuration["Backup:ContainerName"] ?? defaults.ContainerName,
            Database = GetRequiredVariable("POSTGRES_DB"),
            Username = GetRequiredVariable("POSTGRES_USER"),
            Password = GetRequiredVariable("POSTGRES_PASSWORD")
        };
    }

    private static string GetRequiredVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"La variable de entorno '{name}' es obligatoria.");
        }

        return value;
    }
}
