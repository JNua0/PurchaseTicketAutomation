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
using PurchaseTicket.Infrastructure.Persistence;
using PurchaseTicket.Infrastructure.Persistence.Repositories;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.Views.Suppliers;
using PurchaseTicket.Desktop.ViewModels;
using PurchaseTicket.Desktop.ViewModels.Home;
using PurchaseTicket.Desktop.ViewModels.Suppliers;

namespace PurchaseTicket.Desktop;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;


    public App()
    {
        EnvironmentLoader.Load();

        _host = Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                var connectionString = DatabaseConfiguration.GetConnectionString();

                services.AddDbContext<ApplicationDbContext>(
                    options =>
                        options.UseNpgsql(connectionString));

                // Repositories
                services.AddScoped<ISupplierQueryRepository, SupplierQueryRepository>();
                services.AddScoped<ISupplierRepository, SupplierRepository>();

                // Use cases
                services.AddScoped<GetSuppliers>();
                services.AddScoped<CreateSupplier>();
                services.AddScoped<UpdateSupplier>();
                services.AddTransient<SuppliersViewModel>();
                services.AddTransient<MainWindow>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<SuppliersViewModel>();
                services.AddTransient<SupplierFormViewModel>();
                services.AddTransient<SupplierFormWindow>();
                services.AddTransient<ISupplierDialogService, SupplierDialogService>();
                services.AddScoped<ActivateSupplier>();
                services.AddScoped<DeactivateSupplier>();

                services.AddSingleton<IConfirmationDialogService, ConfirmationDialogService>();
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
