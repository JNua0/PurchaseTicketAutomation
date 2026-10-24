namespace PurchaseTicket.Infrastructure.Backups;

// Como llegar a la base: PostgreSQL corre en un contenedor de Docker y
// pg_dump se ejecuta dentro de el, asi no hace falta instalarlo en Windows
public sealed class DatabaseBackupOptions
{
    // Propiedades

    // "docker" basta si Docker Desktop esta en el PATH
    public string DockerPath { get; init; } = "docker";

    // El container_name de compose.yaml
    public string ContainerName { get; init; } = "purchase-ticket-postgres";

    public string Database { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
