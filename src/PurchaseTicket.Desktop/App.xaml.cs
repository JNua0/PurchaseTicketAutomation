using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using PurchaseTicket.Application.UseCases.Materials.Activate;
using PurchaseTicket.Application.UseCases.Materials.Create;
using PurchaseTicket.Application.UseCases.Materials.Deactivate;
using PurchaseTicket.Application.UseCases.Materials.Get;
using PurchaseTicket.Application.UseCases.Materials.GetActive;
using PurchaseTicket.Application.UseCases.Materials.Update;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Cancel;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateConventional;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateSingle;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Print;
using PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterAmount;
using PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterDepartureWeight;
using PurchaseTicket.Application.UseCases.Suppliers.Activate;
using PurchaseTicket.Application.UseCases.Suppliers.Create;
using PurchaseTicket.Application.UseCases.Suppliers.Deactivate;
using PurchaseTicket.Application.UseCases.Suppliers.Get;
using PurchaseTicket.Application.UseCases.Suppliers.GetActive;
using PurchaseTicket.Application.UseCases.Suppliers.Update;
using PurchaseTicket.Application.UseCases.Weighing.ReadWeight;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels;
using PurchaseTicket.Desktop.ViewModels.Home;
using PurchaseTicket.Desktop.ViewModels.Materials;
using PurchaseTicket.Desktop.ViewModels.Settings;
using PurchaseTicket.Desktop.ViewModels.Suppliers;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;
using PurchaseTicket.Desktop.Views.Materials;
using PurchaseTicket.Desktop.Views.Suppliers;
using PurchaseTicket.Desktop.Views.VehicleWeighing;
using PurchaseTicket.Infrastructure.Persistence;
using PurchaseTicket.Infrastructure.Persistence.Repositories;
using PurchaseTicket.Infrastructure.Printing;
using PurchaseTicket.Infrastructure.TicketNumbers;
using PurchaseTicket.Infrastructure.Weighing;
using Serilog;

namespace PurchaseTicket.Desktop;

public partial class App : System.Windows.Application
{
    // Campos

    private readonly IHost _host;

    // Evita abrir un mensaje de error dentro de otro
    private bool _isShowingErrorMessage;

    // Constructor

    public App()
    {
        LoggingConfiguration.Initialize();

        RegisterGlobalExceptionHandlers();

        EnvironmentLoader.Load();

        var configuration = ApplicationSettings.Load();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                var connectionString = DatabaseConfiguration.GetConnectionString();

                // Database
                services.AddDbContext<ApplicationDbContext>(
                    options => options.UseNpgsql(connectionString));

                // General services
                services.AddSingleton<
                    IConfirmationDialogService,
                    ConfirmationDialogService>();

                // Avisos breves: una sola instancia para toda la aplicacion
                services.AddSingleton<NotificationService>();
                services.AddSingleton<INotificationService>(
                    provider => provider.GetRequiredService<NotificationService>());

                // Main window and view models
                services.AddTransient<MainWindow>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<HomeViewModel>();

                // Supplier repositories
                services.AddScoped<ISupplierRepository, SupplierRepository>();
                services.AddScoped<ISupplierQueryRepository, SupplierQueryRepository>();

                // Supplier use cases
                services.AddScoped<GetSuppliers>();
                services.AddScoped<CreateSupplier>();
                services.AddScoped<UpdateSupplier>();
                services.AddScoped<ActivateSupplier>();
                services.AddScoped<DeactivateSupplier>();

                // Supplier view models
                services.AddTransient<SuppliersViewModel>();
                services.AddTransient<SupplierFormViewModel>();

                // Supplier dialogs
                services.AddTransient<SupplierFormWindow>();
                services.AddTransient<ISupplierDialogService, SupplierDialogService>();

                // Material repositories
                services.AddScoped<IMaterialRepository, MaterialRepository>();
                services.AddScoped<IMaterialQueryRepository, MaterialQueryRepository>();

                // Material use cases
                services.AddScoped<GetMaterials>();
                services.AddScoped<CreateMaterial>();
                services.AddScoped<UpdateMaterial>();
                services.AddScoped<ActivateMaterial>();
                services.AddScoped<DeactivateMaterial>();

                // Material view models
                services.AddTransient<MaterialsViewModel>();
                services.AddTransient<MaterialFormViewModel>();

                // Material dialogs
                services.AddTransient<MaterialFormWindow>();
                services.AddTransient<IMaterialDialogService, MaterialDialogService>();

                // Vehicle weighing view models
                services.AddTransient<VehicleWeighingViewModel>();

                // Active catalogs
                services.AddScoped<GetActiveSuppliers>();
                services.AddScoped<GetActiveMaterials>();

                // Purchase ticket repositories
                services.AddScoped<IPurchaseTicketRepository, PurchaseTicketRepository>();
                services.AddScoped<IPurchaseTicketQueryRepository, PurchaseTicketQueryRepository>();

                // Purchase ticket use cases
                services.AddScoped<GetPurchaseTickets>();
                services.AddScoped<CreateConventionalPurchaseTicket>();
                services.AddScoped<CreateSinglePurchaseTicket>();
                services.AddScoped<RegisterDepartureWeight>();
                services.AddScoped<CancelPurchaseTicket>();
                services.AddScoped<RegisterAmount>();
                services.AddScoped<ReadWeightUseCase>();

                // Weighing form
                services.AddTransient<WeightCaptureViewModel>();
                services.AddTransient<CreateWeighingViewModel>();
                services.AddTransient<CreateWeighingWindow>();

                // Registro de tara
                services.AddTransient<RegisterTareViewModel>();
                services.AddTransient<RegisterTareWindow>();

                // Weighing dialogs
                services.AddTransient<IWeighingDialogService, WeighingDialogService>();

                // Generacion de folios
                services.AddScoped<ITicketNumberGenerator, TicketNumberGenerator>();

                // Configuracion del indicador
                services.AddWeightIndicator(configuration);

                // Servicios de impresion
                services.AddTicketPrinter(configuration);
                services.AddScoped<PrintPurchaseTicket>();
                services.AddSingleton<ITicketPrintService, TicketPrintService>();

                // Correccion de tickets
                services.AddScoped<CorrectPurchaseTicket>();

                // ViewModel de configuracion
                services.AddTransient<SettingsViewModel>();

                // Configuracion de la aplicacion
                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton<ApplicationSettingsService>();
            })
            .Build();
    }

    // Metodos protegidos

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        // Escribir en disco los mensajes pendientes antes de cerrar
        Log.CloseAndFlush();

        base.OnExit(e);
    }

    // Metodos privados

    private static void OnDomainUnhandledException(
        object sender,
        UnhandledExceptionEventArgs e)
    {
        Log.Fatal(
            e.ExceptionObject as Exception,
            "Error no controlado. La aplicación se cerrará.");

        // La aplicacion se cerrara: escribir en disco antes
        Log.CloseAndFlush();
    }

    private static void OnUnobservedTaskException(
        object? sender,
        UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(
            e.Exception,
            "Una tarea en segundo plano falló sin ser observada.");

        // Marca la excepcion como atendida
        e.SetObserved();
    }

    // Registra los errores que ningun try/catch atrapo
    private void RegisterGlobalExceptionHandlers()
    {
        // Errores en el hilo de la interfaz
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Errores en otros hilos o durante el arranque
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

        // Tareas asincronas que fallaron sin que nadie esperara su resultado
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        // Si el error se repite mientras se muestra el mensaje, por ejemplo
        // al dibujar una vista, la interfaz no puede recuperarse: se deja
        // que la aplicacion se cierre en lugar de abrir mensajes sin fin
        if (_isShowingErrorMessage)
        {
            Log.Fatal(
                e.Exception,
                "Error repetido en la interfaz. La aplicación se cerrará.");

            return;
        }

        Log.Error(
            e.Exception,
            "Error no controlado en la interfaz.");

        _isShowingErrorMessage = true;

        try
        {
            MessageBox.Show(
                "Ocurrió un error inesperado y la operación no se completó.\n\n" +
                "El detalle quedó registrado en:\n" +
                LoggingConfiguration.LogDirectory,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _isShowingErrorMessage = false;
        }

        // Evita que la aplicacion se cierre
        e.Handled = true;
    }
}