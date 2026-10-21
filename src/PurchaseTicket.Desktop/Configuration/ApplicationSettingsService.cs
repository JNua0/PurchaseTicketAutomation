using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace PurchaseTicket.Desktop.Configuration;

// Lee la configuracion y guarda los cambios del operador en
// appsettings.local.json, que tiene prioridad sobre appsettings.json
public sealed class ApplicationSettingsService
{
    // Campos

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    private readonly IConfiguration _configuration;
    private readonly string _localSettingsPath;

    // Constructor

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

    // Metodos publicos

    public string? GetValue(string key)
    {
        return _configuration[key];
    }

    // Puerto de un dispositivo; section es "Printer" o "WeightIndicator"
    public void SavePort(string section, string port)
    {
        Update(root =>
        {
            var device = root[section] as JsonObject
                ?? new JsonObject();

            device["PortName"] = port;
            root[section] = device;
        });
    }

    public void SaveTicketHeader(TicketHeaderSettings header)
    {
        ArgumentNullException.ThrowIfNull(header);

        Update(root =>
        {
            var ticketHeader = root["TicketHeader"] as JsonObject
                ?? new JsonObject();

            ticketHeader["CompanyName"] = header.CompanyName.Trim();
            ticketHeader["Address"] = header.Address.Trim();
            ticketHeader["Neighborhood"] = header.Neighborhood.Trim();
            ticketHeader["PhoneNumber"] = header.PhoneNumber.Trim();
            ticketHeader["Email"] = header.Email.Trim();

            root["TicketHeader"] = ticketHeader;
        });
    }

    public TicketHeaderSettings GetTicketHeader()
    {
        return new TicketHeaderSettings
        {
            CompanyName = GetValue("TicketHeader:CompanyName") ?? string.Empty,
            Address = GetValue("TicketHeader:Address") ?? string.Empty,
            Neighborhood = GetValue("TicketHeader:Neighborhood") ?? string.Empty,
            PhoneNumber = GetValue("TicketHeader:PhoneNumber") ?? string.Empty,
            Email = GetValue("TicketHeader:Email") ?? string.Empty
        };
    }

    // Metodos privados

    // Lee el archivo local, aplica el cambio y lo escribe completo. Despues
    // recarga la configuracion: la impresora y el indicador leen sus opciones
    // cada vez que se usan, asi el cambio aplica sin reiniciar la aplicacion
    private void Update(Action<JsonObject> change)
    {
        JsonObject root = File.Exists(_localSettingsPath)
            ? JsonNode.Parse(File.ReadAllText(_localSettingsPath)) as JsonObject ?? new JsonObject()
            : new JsonObject();

        change(root);

        File.WriteAllText(
            _localSettingsPath,
            root.ToJsonString(WriteOptions));

        if (_configuration is IConfigurationRoot configurationRoot)
            configurationRoot.Reload();
    }
}