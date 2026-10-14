using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.Suppliers.Get;
using PurchaseTicket.Application.UseCases.Suppliers.Create;
using PurchaseTicket.Application.UseCases.Suppliers.Update;
using PurchaseTicket.Application.UseCases.Suppliers.Activate;
using PurchaseTicket.Application.UseCases.Suppliers.Deactivate;
using PurchaseTicket.Application.UseCases.Materials.Get;
using PurchaseTicket.Application.UseCases.Materials.Create;
using PurchaseTicket.Application.UseCases.Materials.Update;
using PurchaseTicket.Application.UseCases.Materials.Activate;
using PurchaseTicket.Application.UseCases.Materials.Deactivate;
using PurchaseTicket.Infrastructure.Persistence;
using PurchaseTicket.Infrastructure.Persistence.Repositories;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.Views.Suppliers;
using PurchaseTicket.Desktop.ViewModels;
using PurchaseTicket.Desktop.ViewModels.Home;
using PurchaseTicket.Desktop.ViewModels.Suppliers;
using PurchaseTicket.Desktop.Views.Materials;
using PurchaseTicket.Desktop.ViewModels.Materials;

namespace PurchaseTicket.Desktop;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;


    public App()
    {
        EnvironmentLoader.Load();

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
