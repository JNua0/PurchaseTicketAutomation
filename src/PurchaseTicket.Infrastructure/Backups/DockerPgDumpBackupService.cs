using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Backups;

namespace PurchaseTicket.Infrastructure.Backups;

// Respaldo con "docker exec <contenedor> pg_dump" en formato custom, que se
// restaura con pg_restore. Se escribe primero un ".partial" y se renombra al
// terminar bien: la carpeta nunca tiene un respaldo a medias
public sealed class DockerPgDumpBackupService : IDatabaseBackupService
{
    // Campos

    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);

    private readonly DatabaseBackupOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DockerPgDumpBackupService> _logger;

    // Constructor

    public DockerPgDumpBackupService(
        DatabaseBackupOptions options,
        TimeProvider timeProvider,
        ILogger<DockerPgDumpBackupService> logger)
    {
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    // Metodos publicos

    // Traduce el error de docker o pg_dump a un mensaje para el operador
    public static string DescribeError(string standardError, string containerName)
    {
        if (standardError.Contains("No such container", StringComparison.OrdinalIgnoreCase))
            return $"No se encontró el contenedor {containerName}. Revisa en Docker Desktop que la base de datos exista.";

        if (standardError.Contains("is not running", StringComparison.OrdinalIgnoreCase))
            return "El contenedor de la base de datos está detenido. Inícialo en Docker Desktop y vuelve a intentar.";

        if (standardError.Contains("error during connect", StringComparison.OrdinalIgnoreCase)
            || standardError.Contains("Cannot connect to the Docker daemon", StringComparison.OrdinalIgnoreCase)
            || standardError.Contains("docker_engine", StringComparison.OrdinalIgnoreCase))
        {
            return "Docker Desktop no está abierto. Ábrelo y vuelve a intentar.";
        }

        string? firstLine = standardError
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (firstLine is null)
            return "El respaldo terminó con un error desconocido.";

        if (firstLine.Length > 200)
            firstLine = firstLine[..200];

        return $"El respaldo terminó con un error: {firstLine}";
    }

    public async Task<BackupFile> CreateAsync(string folder, CancellationToken cancellationToken = default)
    {
        string path = Path.Combine(folder, BackupFileNames.For(_timeProvider.GetLocalNow().DateTime));
        string partialPath = path + ".partial";

        using var process = new Process { StartInfo = CreateStartInfo() };

        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            throw new BackupFailedException(
                "No se encontró Docker. Revisa que Docker Desktop esté instalado.",
                ex);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);

        try
        {
            // stderr se lee a la par: si se llena su bufer, pg_dump se detiene
            var errorTask = process.StandardError.ReadToEndAsync(timeout.Token);

            await using (var file = new FileStream(partialPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await process.StandardOutput.BaseStream.CopyToAsync(file, timeout.Token);
            }

            await process.WaitForExitAsync(timeout.Token);

            string standardError = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError(
                    "pg_dump termino con codigo {ExitCode}: {StandardError}",
                    process.ExitCode,
                    standardError);

                throw new BackupFailedException(DescribeError(standardError, _options.ContainerName));
            }

            long size = new FileInfo(partialPath).Length;

            if (size == 0)
                throw new BackupFailedException("El respaldo quedó vacío. Revisa que la base de datos esté corriendo.");

            File.Move(partialPath, path, overwrite: true);

            return new BackupFile(path, File.GetLastWriteTimeUtc(path), size);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new BackupFailedException("El respaldo tardó más de 10 minutos y se canceló.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new BackupFailedException(
                $"No se pudo escribir en la carpeta {folder}. Revisa que exista y que haya espacio.",
                ex);
        }
        finally
        {
            StopIfRunning(process);
            DeletePartial(partialPath);
        }
    }

    // Metodos privados

    // La contrasena va por variable de entorno, no en la linea de comandos:
    // "-e PGPASSWORD" sin valor la toma del entorno de docker
    private ProcessStartInfo CreateStartInfo()
    {
        var startInfo = new ProcessStartInfo(_options.DockerPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("exec");
        startInfo.ArgumentList.Add("-e");
        startInfo.ArgumentList.Add("PGPASSWORD");
        startInfo.ArgumentList.Add(_options.ContainerName);
        startInfo.ArgumentList.Add("pg_dump");
        startInfo.ArgumentList.Add("--username");
        startInfo.ArgumentList.Add(_options.Username);
        startInfo.ArgumentList.Add("--dbname");
        startInfo.ArgumentList.Add(_options.Database);
        startInfo.ArgumentList.Add("--format=custom");
        startInfo.ArgumentList.Add("--no-owner");

        startInfo.Environment["PGPASSWORD"] = _options.Password;

        return startInfo;
    }

    private void StopIfRunning(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible detener el proceso de respaldo.");
        }
    }

    private void DeletePartial(string partialPath)
    {
        try
        {
            if (File.Exists(partialPath))
                File.Delete(partialPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible borrar el respaldo incompleto {Path}.", partialPath);
        }
    }
}
