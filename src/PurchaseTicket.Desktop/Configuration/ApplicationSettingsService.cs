using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace PurchaseTicket.Desktop.Configuration;

public sealed class ApplicationSettingsService
{
    private readonly IConfiguration _configuration;
    private readonly string _localSettingsPath;

    public ApplicationSettingsService(IConfiguration configuration)
    {
        _configuration = configuration;

        var localDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PurchaseTicketAutomation");

        Directory.CreateDirectory(localDirectory);

        _localSettingsPath = Path.Combine(
            localDirectory,
            "appsettings.local.json");
    }

    // Obtener un valor de configuracion
    public string? GetValue(string key)
    {
        return _configuration[key];
    }

    // Guardar la configuracion general
    public void SaveSettings(
        string? printerPort,
        string? weightIndicatorPort,
        TicketHeaderSettings header)
    {
        ArgumentNullException.ThrowIfNull(header);

        // Leer la configuracion local existente
        JsonObject root;

        if (File.Exists(_localSettingsPath))
        {
            string json = File.ReadAllText(_localSettingsPath);

            root = JsonNode.Parse(json) as JsonObject
                ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        // Actualizar el puerto de la impresora
        var printer = root["Printer"] as JsonObject
            ?? new JsonObject();

        printer["PortName"] = printerPort ?? string.Empty;
        root["Printer"] = printer;

        // Actualizar el puerto del indicador
        var indicator = root["WeightIndicator"] as JsonObject
            ?? new JsonObject();

        indicator["PortName"] = weightIndicatorPort ?? string.Empty;
        root["WeightIndicator"] = indicator;

        // Actualizar el encabezado del ticket
        var ticketHeader = root["TicketHeader"] as JsonObject
            ?? new JsonObject();

        ticketHeader["CompanyName"] =
            (header.CompanyName ?? string.Empty).Trim();

        ticketHeader["Address"] =
            (header.Address ?? string.Empty).Trim();

        ticketHeader["Neighborhood"] =
            (header.Neighborhood ?? string.Empty).Trim();

        ticketHeader["PhoneNumber"] =
            (header.PhoneNumber ?? string.Empty).Trim();

        ticketHeader["Email"] =
            (header.Email ?? string.Empty).Trim();

        root["TicketHeader"] = ticketHeader;

        // Guardar todos los cambios
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(
            _localSettingsPath,
            root.ToJsonString(options));

        // Recargar la configuracion despues de guardar
        if (_configuration is IConfigurationRoot configurationRoot)
        {
            configurationRoot.Reload();
        }
    }
}
