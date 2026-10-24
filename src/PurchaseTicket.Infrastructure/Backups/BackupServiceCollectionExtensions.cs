using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PurchaseTicket.Application.Abstractions.Backups;

namespace PurchaseTicket.Infrastructure.Backups;

public static class BackupServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseBackup(
        this IServiceCollection services,
        DatabaseBackupOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);

        // Reloj del sistema, para el nombre del archivo en hora local
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<IDatabaseBackupService, DockerPgDumpBackupService>();
        services.AddSingleton<IBackupStorage, FileBackupStorage>();

        return services;
    }
}
