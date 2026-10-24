using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.UseCases.Backups;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.Settings;

// Seccion Respaldos de Configuracion: estado del ultimo respaldo, la carpeta
// y el boton Respaldar ahora. El estado vive en BackupScheduler
public sealed class BackupSectionViewModel : ObservableObject
{
    // Campos

    private readonly BackupScheduler _scheduler;
    private readonly ISettingsDialogService _dialogs;
    private readonly INotificationService _notifications;
    private readonly TimeProvider _clock;
    private readonly ILogger<BackupSectionViewModel> _logger;

    // Constructor

    public BackupSectionViewModel(
        BackupScheduler scheduler,
        ISettingsDialogService dialogs,
        INotificationService notifications,
        TimeProvider clock,
        ILogger<BackupSectionViewModel> logger)
    {
        _scheduler = scheduler;
        _dialogs = dialogs;
        _notifications = notifications;
        _clock = clock;
        _logger = logger;

        BackupNowCommand = new AsyncRelayCommand(BackupNowAsync, () => !_scheduler.IsRunning);
        ChangeFolderCommand = new RelayCommand(ChangeFolder);
        OpenFolderCommand = new RelayCommand(OpenFolder);

        _scheduler.Changed += OnSchedulerChanged;
    }

    // Propiedades

    public string Description =>
        $"Uno al día, al abrir la app o cada hora mientras está abierta. Se conservan los últimos {BackupSchedule.KeepCount}.";

    public string StatusText => _scheduler.Status.Text;

    public StatusKind StatusKind => _scheduler.Status.Kind;

    public string LastBackupText =>
        _scheduler.Latest is { } latest ? FormatDate(latest.CreatedAtUtc) : "Ninguno";

    public string SizeText =>
        _scheduler.Latest is { } latest ? FormatSize(latest.SizeBytes) : "—";

    public string Folder => _scheduler.Folder;

    public string ErrorMessage => _scheduler.LastError ?? string.Empty;

    public bool HasError => _scheduler.LastError is not null;

    public bool IsRunning => _scheduler.IsRunning;

    // Comandos

    public AsyncRelayCommand BackupNowCommand { get; }

    public RelayCommand ChangeFolderCommand { get; }

    public RelayCommand OpenFolderCommand { get; }

    // Metodos publicos

    // Al entrar a Configuracion: el respaldo pudo crearse o borrarse por fuera
    public void Refresh()
    {
        _scheduler.Refresh();
    }

    // Metodos privados

    // "1.8 MB" o "820 KB"
    private static string FormatSize(long bytes)
    {
        const double Kilobyte = 1024;
        const double Megabyte = Kilobyte * 1024;

        return bytes >= Megabyte
            ? $"{(bytes / Megabyte).ToString("0.0", CultureInfo.CurrentCulture)} MB"
            : $"{Math.Max(1, Math.Round(bytes / Kilobyte)).ToString("0", CultureInfo.CurrentCulture)} KB";
    }

    // "Hoy 08:02", "Ayer 08:02" o "20/10/2026 08:05"
    private string FormatDate(DateTime utc)
    {
        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc),
            _clock.LocalTimeZone);

        DateTime today = TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), _clock.LocalTimeZone).Date;

        string time = local.ToString("HH:mm", CultureInfo.CurrentCulture);

        if (local.Date == today)
            return $"Hoy {time}";

        if (local.Date == today.AddDays(-1))
            return $"Ayer {time}";

        return local.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
    }

    private void OnSchedulerChanged(object? sender, EventArgs e)
    {
        // Todas las propiedades salen del programador: se avisan juntas
        OnPropertyChanged(string.Empty);

        BackupNowCommand.NotifyCanExecuteChanged();
    }

    private async Task BackupNowAsync()
    {
        var backup = await _scheduler.RunNowAsync();

        if (backup is not null)
            _notifications.ShowSuccess($"Respaldo guardado: {Path.GetFileName(backup.Path)} ({FormatSize(backup.SizeBytes)}).");
        else if (_scheduler.LastError is not null)
            _notifications.ShowError("No se pudo hacer el respaldo. El motivo está en Configuración.");
    }

    // Mientras respalda no se cambia: el archivo va a la carpeta anterior
    private void ChangeFolder()
    {
        if (_scheduler.IsRunning)
        {
            _notifications.ShowInfo("Espera a que termine el respaldo para cambiar la carpeta.");
            return;
        }

        string? folder = _dialogs.ShowBackupFolderDialog(_scheduler.Folder);

        if (folder is null || string.Equals(folder, _scheduler.Folder, StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            _scheduler.ChangeFolder(folder);
        }
        catch (Exception ex)
        {
            _notifications.ShowError("No fue posible guardar la carpeta de respaldos.");

            _logger.LogError(ex, "No fue posible guardar la carpeta de respaldos {Folder}.", folder);
            return;
        }

        _notifications.ShowSuccess($"Carpeta de respaldos guardada: {folder}");
    }

    private void OpenFolder()
    {
        try
        {
            _dialogs.OpenFolder(_scheduler.Folder);
        }
        catch (Exception ex)
        {
            _notifications.ShowError("No fue posible abrir la carpeta. Revisa que exista.");

            _logger.LogWarning(ex, "No fue posible abrir la carpeta de respaldos {Folder}.", _scheduler.Folder);
        }
    }
}
