using PurchaseTicket.Application.Abstractions.Backups;

namespace PurchaseTicket.Application.UseCases.Backups;

// Cada cuanto se respalda y cuantos respaldos se conservan
public static class BackupSchedule
{
    // Campos

    public const int KeepCount = 30;

    public static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    // Metodos publicos

    // Sin respaldos, o el ultimo ya tiene un dia
    public static bool IsDue(BackupFile? latest, DateTime nowUtc)
    {
        return latest is null || nowUtc - latest.CreatedAtUtc >= Interval;
    }
}
