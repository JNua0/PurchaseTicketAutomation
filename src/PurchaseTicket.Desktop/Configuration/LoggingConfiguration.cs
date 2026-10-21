using System.IO;
using Serilog;
using Serilog.Events;

namespace PurchaseTicket.Desktop.Configuration;

public static class LoggingConfiguration
{
    // Carpeta de registros: %LOCALAPPDATA%\PurchaseTicketAutomation\logs
    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "PurchaseTicketAutomation",
        "logs");

    public static void Initialize()
    {
        Log.Logger = new LoggerConfiguration()

            // Para el codigo propio se registran advertencias y errores
            .MinimumLevel.Warning()

            // Para librerias de Microsoft y System solo errores
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("System", LogEventLevel.Error)
            .WriteTo.File(
                path: Path.Combine(LogDirectory, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
                    "{SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}