using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.Abstractions.Reports;

namespace PurchaseTicket.Infrastructure.Reports;

public static class ReportServiceCollectionExtensions
{
    public static IServiceCollection AddReportExport(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPurchaseReportExporter, PurchaseReportExporter>();

        return services;
    }
}
