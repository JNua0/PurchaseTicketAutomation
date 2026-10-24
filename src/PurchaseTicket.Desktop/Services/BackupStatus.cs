using PurchaseTicket.Desktop.Components.Feedback;

namespace PurchaseTicket.Desktop.Services;

// Estado del respaldo para Configuracion (Text) y para Inicio (HomeText).
// Inicio solo lo muestra si necesita atencion
public sealed record BackupStatus(
    string Text,
    string HomeText,
    StatusKind Kind)
{
    // Campos

    // Desde este tiempo sin respaldo se avisa en ambar
    public static readonly TimeSpan OverdueAfter = TimeSpan.FromDays(2);

    // Propiedades

    public bool NeedsAttention => Kind is StatusKind.Warning or StatusKind.Error;

    // Metodos publicos

    public static BackupStatus Create(
        DateTime? latestUtc,
        string? lastError,
        bool isRunning,
        DateTime nowUtc)
    {
        if (isRunning)
            return new("Respaldando…", "Respaldo · en curso", StatusKind.Neutral);

        if (lastError is not null)
            return new("Falló el respaldo", "Respaldo · falló", StatusKind.Error);

        if (latestUtc is not DateTime latest)
            return new("Sin respaldos", "Respaldo · nunca se ha hecho", StatusKind.Warning);

        TimeSpan age = nowUtc - latest;

        if (age >= OverdueAfter)
        {
            int days = (int)age.TotalDays;

            return new($"Sin respaldo hace {days} días", $"Respaldo · hace {days} días", StatusKind.Warning);
        }

        return new("Al día", "Respaldo · al día", StatusKind.Success);
    }
}
