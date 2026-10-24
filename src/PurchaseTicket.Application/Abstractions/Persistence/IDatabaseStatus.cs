namespace PurchaseTicket.Application.Abstractions.Persistence;

// Lo que la app revisa al abrir: si la base responde y si le faltan
// migraciones. No crea ni inicia nada fuera de la propia base
public interface IDatabaseStatus
{
    // Falso si la base no responde en el tiempo dado
    Task<bool> CanConnectAsync(TimeSpan timeout);

    // Migraciones que la base todavia no tiene, de la mas antigua a la mas nueva
    Task<IReadOnlyList<string>> GetPendingMigrationsAsync();

    // Cuantas ya tiene; cero en una base recien creada
    Task<int> CountAppliedMigrationsAsync();

    Task ApplyMigrationsAsync();
}
