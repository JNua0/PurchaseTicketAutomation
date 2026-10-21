using System.IO;
using Microsoft.Extensions.Configuration;

namespace PurchaseTicket.Desktop.Configuration;

public static class ApplicationSettings
{
    public static IConfigurationRoot Load()
    {
        var localDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PurchaseTicketAutomation");

        Directory.CreateDirectory(localDirectory);

        var localSettingsPath = Path.Combine(
            localDirectory,
            "appsettings.local.json");

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: false)
            .AddJsonFile(
                localSettingsPath,
                optional: true,
                reloadOnChange: true)
            .Build();
    }
}
