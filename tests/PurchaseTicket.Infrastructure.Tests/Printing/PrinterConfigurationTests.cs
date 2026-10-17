
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Infrastructure.Printing;
using System.Text.Json;

namespace PurchaseTicket.Infrastructure.Tests.Printing;

public class PrinterConfigurationTests
{
    [Fact]
    public void TicketPrinterOptions_ShouldUseUpdatedConfiguration_AfterReload()
    {
        // Arrange
        string directory = Path.Combine(
            Path.GetTempPath(),
            "PurchaseTicketTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        string settingsPath = Path.Combine(
            directory,
            "appsettings.local.json");

        try
        {
            // Crear la configuracion inicial
            WriteSettings(
                settingsPath,
                "Empresa original",
                "Direccion original");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(
                    settingsPath,
                    optional: false,
                    reloadOnChange: false)
                .Build();

            var services = new ServiceCollection();

            services.AddTicketPrinter(configuration);

            using var provider = services.BuildServiceProvider();

            // Obtener las opciones originales
            var originalOptions =
                provider.GetRequiredService<TicketPrinterOptions>();

            Assert.Equal(
                "Empresa original",
                originalOptions.CompanyName);

            // Act
            // Modificar la configuracion local
            WriteSettings(
                settingsPath,
                "EcoPapel Recycling",
                "Nueva direccion");

            // Recargar los valores en memoria
            configuration.Reload();

            // Resolver una nueva instancia
            var updatedOptions =
                provider.GetRequiredService<TicketPrinterOptions>();

            // Assert
            Assert.NotSame(originalOptions, updatedOptions);

            Assert.Equal(
                "EcoPapel Recycling",
                updatedOptions.CompanyName);

            Assert.Equal(
                "Nueva direccion",
                updatedOptions.Address);

            // La instancia anterior conserva sus valores
            Assert.Equal(
                "Empresa original",
                originalOptions.CompanyName);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    // Crear un archivo temporal de configuracion
    private static void WriteSettings(
        string path,
        string companyName,
        string address)
    {
        var settings = new
        {
            TicketHeader = new
            {
                CompanyName = companyName,
                Address = address,
                Neighborhood = "Colonia Centro",
                PhoneNumber = "0000000000",
                Email = "contacto@example.com"
            },
            Printer = new
            {
                PortName = "COM4",
                BaudRate = 9600,
                DataBits = 8,
                Parity = "None",
                StopBits = "One",
                Handshake = "None"
            }
        };

        File.WriteAllText(
            path,
            JsonSerializer.Serialize(settings));
    }
}
