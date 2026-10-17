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
using PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateConventional;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateSingle;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Print;
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
using System.Windows;

namespace PurchaseTicket.Desktop;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;


    public App()
    {
        EnvironmentLoader.Load();

        var configuration = ApplicationSettings.Load();

        _host = Host.CreateDefaultBuilder()
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

                // Purchase ticket repositories
                services.AddScoped<IPurchaseTicketQueryRepository, PurchaseTicketQueryRepository>();

                // Purchase ticket use cases
                services.AddScoped<GetPurchaseTickets>();

                // Vehicle weighing view models
                services.AddTransient<VehicleWeighingViewModel>();

                // Active catalogs
                services.AddScoped<GetActiveSuppliers>();
                services.AddScoped<GetActiveMaterials>();

                // Purchase ticket repositories
                services.AddScoped<IPurchaseTicketRepository, PurchaseTicketRepository>();

                // Purchase ticket use cases
                services.AddScoped<CreateConventionalPurchaseTicket>();
                services.AddScoped<CreateSinglePurchaseTicket>();
                services.AddScoped<ReadWeightUseCase>();

                // Weighing form
                services.AddTransient<CreateWeighingViewModel>();
                services.AddTransient<CreateWeighingWindow>();

                // Weighing dialogs
                services.AddTransient<IWeighingDialogService, WeighingDialogService>();

                // Generacion de folios
                services.AddScoped<ITicketNumberGenerator, TicketNumberGenerator>();

                // Configuracion del indicador
                services.AddWeightIndicator(configuration);

                // Servicios de impresion
                services.AddTicketPrinter(configuration);
                services.AddScoped<PrintPurchaseTicket>();

                // Correccion de tickets
                services.AddScoped<CorrectPurchaseTicket>();

                // Casos de uso de tickets
                services.AddScoped<CreateConventionalPurchaseTicket>();
                services.AddScoped<CreateSinglePurchaseTicket>();
                services.AddScoped<CorrectPurchaseTicket>();
                services.AddScoped<PrintPurchaseTicket>();

                // ViewModel de configuracion
                services.AddTransient<SettingsViewModel>();

                // Configuracion de la aplicacion
                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton<ApplicationSettingsService>();
            })
            .Build();
    }

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

        base.OnExit(e);
    }
}
