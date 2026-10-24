namespace PurchaseTicket.Application.Abstractions.Backups;

// El respaldo no se pudo crear. Su mensaje se muestra al operador tal cual,
// por ejemplo que Docker Desktop no esta abierto
public sealed class BackupFailedException : Exception
{
    public BackupFailedException(string message)
        : base(message)
    {
    }

    public BackupFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
