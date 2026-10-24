namespace PurchaseTicket.Desktop.ViewModels.Startup;

// En que va el arranque. Los tres primeros muestran el indicador de espera;
// los de error, el motivo y los botones
public enum StartupStep
{
    Connecting,

    BackingUp,

    Migrating,

    ConnectionFailed,

    BackupFailed,

    MigrationFailed
}
