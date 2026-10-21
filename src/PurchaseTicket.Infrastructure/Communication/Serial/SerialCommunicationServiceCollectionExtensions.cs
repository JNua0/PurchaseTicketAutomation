using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.Abstractions.Devices;

namespace PurchaseTicket.Infrastructure.Communication.Serial;

public static class SerialCommunicationServiceCollectionExtensions
{
    public static IServiceCollection AddSerialPortDiscovery(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Sin estado: una sola instancia para toda la aplicacion
        services.AddSingleton<ISerialPortDiscovery, SerialPortDiscovery>();

        return services;
    }
}