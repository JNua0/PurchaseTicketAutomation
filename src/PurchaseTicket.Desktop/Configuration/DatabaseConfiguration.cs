using Npgsql;

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
