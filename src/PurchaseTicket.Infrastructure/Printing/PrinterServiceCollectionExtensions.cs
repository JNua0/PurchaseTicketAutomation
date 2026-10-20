using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Infrastructure.Printing.Serial;

namespace PurchaseTicket.Infrastructure.Printing;

public static class PrinterServiceCollectionExtensions
{
    public static IServiceCollection AddTicketPrinter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configuracion de comunicacion serial
        services.AddTransient<SerialPrinterOptions>(_ =>
        {
            var options = new SerialPrinterOptions();

            configuration
                .GetSection("Printer")
                .Bind(options);

            return options;
        });

        // Configuracion del encabezado del ticket
        services.AddTransient<TicketPrinterOptions>(_ =>
        {
            var options = new TicketPrinterOptions();

            configuration
                .GetSection("TicketHeader")
                .Bind(options);

            return options;
        });

        // Fabrica del puerto serial
        services.AddTransient<
            ISerialPortFactory,
            SerialPortFactory>();

        // Reloj del sistema, usado para convertir las fechas a hora local
        services.TryAddSingleton(TimeProvider.System);

        // Implementacion de la impresora
        services.AddTransient<
            ITicketPrinter,
            EpsonTmU295TicketPrinter>();

        return services;
    }
}