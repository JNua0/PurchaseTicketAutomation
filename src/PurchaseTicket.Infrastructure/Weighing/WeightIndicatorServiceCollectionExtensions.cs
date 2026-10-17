using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.Abstractions.Weighing;
using PurchaseTicket.Infrastructure.Weighing.Serial;

namespace PurchaseTicket.Infrastructure.Weighing;

public static class WeightIndicatorServiceCollectionExtensions
{
    public static IServiceCollection AddWeightIndicator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddTransient<WeightIndicatorOptions>(_ =>
        {
            var options = new WeightIndicatorOptions();

            configuration
                .GetSection("WeightIndicator")
                .Bind(options);

            return options;
        });

        services.AddTransient<
            IWeightIndicatorSerialPortFactory,
            WeightIndicatorSerialPortFactory>();

        services.AddTransient<
            IWeightReader,
            DiniArgeoWeightReader>();

        return services;
    }
}
