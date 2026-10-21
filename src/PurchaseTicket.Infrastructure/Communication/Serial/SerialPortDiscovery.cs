using System.IO.Ports;
using System.Management;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Devices;

namespace PurchaseTicket.Infrastructure.Communication.Serial;

// Combina dos fuentes de Windows: la lista de puertos de .NET, que es confiable
// pero solo trae "COM5", y los dispositivos del Administrador de dispositivos,
// que traen el nombre del adaptador. Si la segunda falla, la lista sigue completa
public sealed class SerialPortDiscovery : ISerialPortDiscovery
{
    // Campos

    // Windows nombra el dispositivo como "USB-SERIAL CH340 (COM5)"
    private static readonly Regex DeviceNamePattern = new(
        @"^(?<description>.*?)\s*\((?<port>COM\d+)\)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Func<IEnumerable<string>> _getPortNames;
    private readonly Func<IEnumerable<string>> _getDeviceNames;
    private readonly ILogger<SerialPortDiscovery> _logger;

    // Constructor

    public SerialPortDiscovery(ILogger<SerialPortDiscovery> logger)
        : this(SerialPort.GetPortNames, QueryDeviceNames, logger)
    {
    }

    // Las pruebas reemplazan las dos fuentes de Windows
    public SerialPortDiscovery(
        Func<IEnumerable<string>> getPortNames,
        Func<IEnumerable<string>> getDeviceNames,
        ILogger<SerialPortDiscovery> logger)
    {
        _getPortNames = getPortNames;
        _getDeviceNames = getDeviceNames;
        _logger = logger;
    }

    // Metodos publicos

    public IReadOnlyList<SerialPortInfo> GetPorts()
    {
        var descriptions = GetDescriptions();

        return _getPortNames()
            .Select(name => name.Trim().ToUpperInvariant())
            .Where(name => name.Length > 0)
            .Distinct()
            .Select(name => new SerialPortInfo(
                name,
                descriptions.GetValueOrDefault(name)))
            .OrderBy(port => PortNumber(port.Name))
            .ThenBy(port => port.Name, StringComparer.Ordinal)
            .ToList();
    }

    // Metodos privados

    // Orden por numero: COM10 va despues de COM9, no despues de COM1
    private static int PortNumber(string name)
    {
        return name.StartsWith("COM", StringComparison.Ordinal)
            && int.TryParse(name[3..], out int number)
                ? number
                : int.MaxValue;
    }

    // Win32_PnPEntity solo existe en Windows; en otro sistema no hay descripciones
    private static IEnumerable<string> QueryDeviceNames()
    {
        return OperatingSystem.IsWindows()
            ? QueryWindowsDeviceNames()
            : [];
    }

    // El atributo le indica al analizador de plataforma que este metodo solo
    // se llama en Windows; asi no marca advertencias por usar WMI
    [SupportedOSPlatform("windows")]
    private static List<string> QueryWindowsDeviceNames()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT Name FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");

        using var results = searcher.Get();

        return results
            .Cast<ManagementBaseObject>()
            .Select(device => device["Name"] as string)
            .OfType<string>()
            .ToList();
    }

    // Puerto y nombre del dispositivo. Si la consulta falla se registra como
    // advertencia: la lista de puertos se muestra igual, sin descripcion
    private Dictionary<string, string> GetDescriptions()
    {
        var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (string deviceName in _getDeviceNames())
            {
                var match = DeviceNamePattern.Match(deviceName);

                if (match.Success)
                {
                    descriptions[match.Groups["port"].Value.ToUpperInvariant()] =
                        match.Groups["description"].Value.Trim();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No fue posible consultar los nombres de los dispositivos serie.");
        }

        return descriptions;
    }
}