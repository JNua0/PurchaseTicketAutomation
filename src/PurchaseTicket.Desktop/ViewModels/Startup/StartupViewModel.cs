using System.IO;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Backups;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Backups.Create;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.Startup;

// Arranque: antes de abrir la ventana principal revisa que la base responda
// y, si le faltan migraciones, la respalda y la actualiza. Si la base no
// responde reintenta solo cada pocos segundos; el operador puede reintentar
// antes o salir
public sealed class StartupViewModel : ObservableObject
{
    // Campos

    private const int RetrySeconds = 10;

    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ApplicationSettingsService _settings;
    private readonly ILogger<StartupViewModel> _logger;
    private readonly DispatcherTimer _retryTimer;

    private StartupStep _step = StartupStep.Connecting;
    private string _message = string.Empty;
    private string _detail = string.Empty;
    private int _secondsToRetry;
    private bool _isRunning;

    // Constructor

    public StartupViewModel(
        IServiceScopeFactory scopeFactory,
        ApplicationSettingsService settings,
        ILogger<StartupViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;

        _retryTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _retryTimer.Tick += OnRetryTick;

        RetryCommand = new AsyncRelayCommand(() => RunAsync(skipBackup: false), () => IsFailed && !_isRunning);
        SkipBackupCommand = new AsyncRelayCommand(() => RunAsync(skipBackup: true), () => CanSkipBackup && !_isRunning);
        ExitCommand = new RelayCommand(() => Finished?.Invoke(this, false));
    }

    // Eventos

    // true: la base esta lista y se abre la app; false: el operador salio
    public event EventHandler<bool>? Finished;

    // Propiedades

    public StartupStep Step
    {
        get => _step;
        private set
        {
            if (!SetProperty(ref _step, value))
                return;

            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsFailed));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(CanSkipBackup));
            OnPropertyChanged(nameof(ShowsRetryCountdown));
            NotifyCommands();
        }
    }

    public bool IsBusy => Step is StartupStep.Connecting or StartupStep.BackingUp or StartupStep.Migrating;

    public bool IsFailed => !IsBusy;

    public string Title => Step switch
    {
        StartupStep.Connecting => "Conectando con la base de datos",
        StartupStep.BackingUp => "Respaldando antes de actualizar",
        StartupStep.Migrating => "Actualizando la base de datos",
        StartupStep.ConnectionFailed => "No se pudo conectar con la base de datos",
        StartupStep.BackupFailed => "No se pudo respaldar antes de actualizar",
        _ => "No se pudo actualizar la base de datos"
    };

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    // Lo que puede revisar el operador, o donde quedo el detalle
    public string Detail
    {
        get => _detail;
        private set
        {
            if (SetProperty(ref _detail, value))
                OnPropertyChanged(nameof(HasDetail));
        }
    }

    public bool HasDetail => Detail.Length > 0;

    // Solo si fallo el respaldo: el operador decide si actualizar sin el
    public bool CanSkipBackup => Step == StartupStep.BackupFailed;

    // Sin conexion se reintenta solo, por ejemplo mientras Docker termina de abrir
    public bool ShowsRetryCountdown => Step == StartupStep.ConnectionFailed;

    public string RetryCountdownText => $"Se vuelve a intentar en {_secondsToRetry} s";

    // Comandos

    public AsyncRelayCommand RetryCommand { get; }

    public AsyncRelayCommand SkipBackupCommand { get; }

    public RelayCommand ExitCommand { get; }

    // Metodos publicos

    // La ventana lo llama al mostrarse, y de nuevo cada reintento
    public async Task RunAsync(bool skipBackup = false)
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _retryTimer.Stop();

        try
        {
            if (!await ConnectAsync())
                return;

            if (await UpdateDatabaseAsync(skipBackup))
                Finished?.Invoke(this, true);
        }
        finally
        {
            _isRunning = false;
            NotifyCommands();
        }
    }

    // La ventana se cierra: no debe quedar el reintento corriendo
    public void Stop()
    {
        _retryTimer.Stop();
    }

    // Metodos privados

    private async Task<bool> ConnectAsync()
    {
        Step = StartupStep.Connecting;
        Message = "Un momento, por favor.";
        Detail = string.Empty;

        bool connected;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            connected = await scope.ServiceProvider
                .GetRequiredService<IDatabaseStatus>()
                .CanConnectAsync(ConnectTimeout);
        }
        catch (Exception ex)
        {
            connected = false;

            _logger.LogWarning(ex, "No fue posible conectar con la base de datos al iniciar.");
        }

        if (connected)
            return true;

        _logger.LogWarning("La base de datos no respondió al iniciar.");

        Step = StartupStep.ConnectionFailed;
        Message = "Revisa que Docker Desktop esté abierto y que la base de datos esté iniciada. Si la computadora acaba de encender, espera un momento.";
        Detail = string.Empty;

        StartRetryCountdown();

        return false;
    }

    // Respalda y migra si hace falta; false si algo fallo y la ventana sigue abierta
    private async Task<bool> UpdateDatabaseAsync(bool skipBackup)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var database = scope.ServiceProvider.GetRequiredService<IDatabaseStatus>();

            var pending = await database.GetPendingMigrationsAsync();

            if (pending.Count == 0)
                return true;

            // Una base recien creada no tiene nada que respaldar
            bool hasData = await database.CountAppliedMigrationsAsync() > 0;

            if (hasData && !skipBackup && !await BackupAsync(scope.ServiceProvider))
                return false;

            Step = StartupStep.Migrating;
            Message = pending.Count == 1
                ? "Se aplica 1 cambio a la base de datos. No cierres la aplicación."
                : $"Se aplican {pending.Count} cambios a la base de datos. No cierres la aplicación.";
            Detail = string.Empty;

            _logger.LogInformation("Aplicando migraciones pendientes: {Migrations}", string.Join(", ", pending));

            await database.ApplyMigrationsAsync();

            _logger.LogInformation("Base de datos actualizada con {Count} migraciones.", pending.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No fue posible actualizar la base de datos al iniciar.");

            Step = StartupStep.MigrationFailed;
            Message = "La aplicación no puede abrir hasta que la base de datos esté actualizada.";
            Detail = "El detalle quedó en el registro de errores.";

            return false;
        }
    }

    // En la carpeta elegida; si no esta disponible, como una USB desconectada,
    // en la carpeta por defecto para no frenar la actualizacion
    private async Task<bool> BackupAsync(IServiceProvider services)
    {
        Step = StartupStep.BackingUp;
        Message = "Se guarda una copia de los datos antes de actualizar.";
        Detail = string.Empty;

        string folder = _settings.GetBackupFolder();

        if (!Directory.Exists(folder))
            folder = _settings.GetDefaultBackupFolder();

        try
        {
            var useCase = services.GetRequiredService<CreateDatabaseBackup>();

            var backup = await Task.Run(() => useCase.ExecuteAsync(folder));

            _logger.LogInformation("Respaldo previo a la actualización en {Path}.", backup.Path);

            return true;
        }
        catch (BackupFailedException ex)
        {
            _logger.LogWarning(ex, "No fue posible respaldar antes de actualizar la base de datos.");

            Step = StartupStep.BackupFailed;
            Message = "Puedes reintentar, o actualizar sin respaldo si estás seguro.";
            Detail = ex.Message;

            return false;
        }
    }

    private void StartRetryCountdown()
    {
        _secondsToRetry = RetrySeconds;
        OnPropertyChanged(nameof(RetryCountdownText));

        _retryTimer.Start();
    }

    private async void OnRetryTick(object? sender, EventArgs e)
    {
        _secondsToRetry--;
        OnPropertyChanged(nameof(RetryCountdownText));

        if (_secondsToRetry > 0)
            return;

        _retryTimer.Stop();

        try
        {
            await RunAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló el reintento de conexión al iniciar.");
        }
    }

    private void NotifyCommands()
    {
        RetryCommand?.NotifyCanExecuteChanged();
        SkipBackupCommand?.NotifyCanExecuteChanged();
    }
}
