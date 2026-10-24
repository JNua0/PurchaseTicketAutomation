using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Backups;
using PurchaseTicket.Application.UseCases.Backups;
using PurchaseTicket.Application.UseCases.Backups.Create;
using PurchaseTicket.Application.UseCases.Backups.GetLatest;
using PurchaseTicket.Desktop.Configuration;

namespace PurchaseTicket.Desktop.Services;

// Respaldo diario mientras la app esta abierta: revisa poco despues de abrir
// y luego cada hora, y respalda si el ultimo tiene un dia. Una sola instancia
// para toda la app: Configuracion e Inicio leen su estado y escuchan Changed
public sealed class BackupScheduler
{
    // Campos

    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ApplicationSettingsService _settings;
    private readonly INotificationService _notifications;
    private readonly TimeProvider _clock;
    private readonly ILogger<BackupScheduler> _logger;
    private readonly DispatcherTimer _timer;

    private string _folder = string.Empty;
    private BackupFile? _latest;
    private string? _lastError;
    private bool _isRunning;
    private bool _isStarted;

    // Constructor

    public BackupScheduler(
        IServiceScopeFactory scopeFactory,
        ApplicationSettingsService settings,
        INotificationService notifications,
        TimeProvider clock,
        ILogger<BackupScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _notifications = notifications;
        _clock = clock;
        _logger = logger;

        _timer = new DispatcherTimer { Interval = CheckInterval };
        _timer.Tick += OnTimerTick;
    }

    // Eventos

    // El estado cambio: inicio o fin de un respaldo, otra carpeta o un error
    public event EventHandler? Changed;

    // Propiedades

    public string Folder => _folder;

    public BackupFile? Latest => _latest;

    // Mensaje del ultimo intento fallido; null despues de uno correcto
    public string? LastError => _lastError;

    public bool IsRunning => _isRunning;

    public BackupStatus Status =>
        BackupStatus.Create(_latest?.CreatedAtUtc, _lastError, _isRunning, _clock.GetUtcNow().UtcDateTime);

    // Metodos publicos

    // Se llama una vez, con la ventana principal ya visible
    public async void Start()
    {
        if (_isStarted)
            return;

        _isStarted = true;

        try
        {
            _folder = _settings.GetBackupFolder();
            Refresh();

            _timer.Start();

            // Sin prisa al abrir: primero que la app cargue
            await Task.Delay(StartupDelay);
            await RunIfDueAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No fue posible iniciar el respaldo automático.");
        }
    }

    // Vuelve a leer el ultimo respaldo de la carpeta, por ejemplo al entrar a Configuracion
    public void Refresh()
    {
        if (string.IsNullOrEmpty(_folder))
            _folder = _settings.GetBackupFolder();

        try
        {
            using var scope = _scopeFactory.CreateScope();

            _latest = scope.ServiceProvider
                .GetRequiredService<GetLatestBackup>()
                .Execute(_folder);
        }
        catch (Exception ex)
        {
            _latest = null;

            _logger.LogError(ex, "No fue posible leer la carpeta de respaldos {Folder}.", _folder);
        }

        OnChanged();
    }

    // El boton Respaldar ahora; el error queda en LastError
    public Task<BackupFile?> RunNowAsync()
    {
        return RunAsync(isAutomatic: false);
    }

    // Se guarda en cuanto se elige; el error anterior era de la otra carpeta
    public void ChangeFolder(string folder)
    {
        _settings.SaveBackupFolder(folder);

        _folder = folder;
        _lastError = null;

        Refresh();
    }

    // Metodos privados

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        try
        {
            await RunIfDueAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló la revisión del respaldo automático.");
        }
    }

    private async Task RunIfDueAsync()
    {
        Refresh();

        if (!BackupSchedule.IsDue(_latest, _clock.GetUtcNow().UtcDateTime))
            return;

        await RunAsync(isAutomatic: true);
    }

    private async Task<BackupFile?> RunAsync(bool isAutomatic)
    {
        // El automatico y el boton pueden coincidir: basta con uno
        if (_isRunning)
            return null;

        _isRunning = true;
        OnChanged();

        string folder = _folder;
        bool wasFailing = _lastError is not null;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var useCase = scope.ServiceProvider.GetRequiredService<CreateDatabaseBackup>();

            // Fuera del hilo de la interfaz: pg_dump puede tardar
            var backup = await Task.Run(() => useCase.ExecuteAsync(folder));

            _latest = backup;
            _lastError = null;

            _logger.LogInformation(
                "Respaldo creado en {Path} ({Size} bytes).",
                backup.Path,
                backup.SizeBytes);

            return backup;
        }
        catch (BackupFailedException ex)
        {
            _lastError = ex.Message;

            _logger.LogWarning(ex, "No fue posible crear el respaldo en {Folder}.", folder);
        }
        catch (Exception ex)
        {
            _lastError = "No fue posible crear el respaldo. El detalle quedó en el registro de errores.";

            _logger.LogError(ex, "Error inesperado al crear el respaldo en {Folder}.", folder);
        }
        finally
        {
            _isRunning = false;
            OnChanged();
        }

        // El boton muestra su propio error. El automatico avisa solo la primera
        // vez: si sigue fallando cada hora, el aviso ya esta en Inicio
        if (isAutomatic && !wasFailing)
            _notifications.ShowWarning("No se pudo hacer el respaldo automático. Revisa Configuración.");

        return null;
    }

    private void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
