using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;

const string portName = "COM4";
const int stableWeightTimeoutSeconds = 5;

Console.WriteLine("Prueba de indicador de peso");
Console.WriteLine();

while (true)
{
    Console.WriteLine("Presiona ENTER para obtener el peso...");
    Console.ReadLine();

    try
    {
        decimal weight = ReadStableWeight();

        Console.WriteLine();
        Console.WriteLine($"Peso obtenido: {weight} kg");
        Console.WriteLine();
    }
    catch (TimeoutException ex)
    {
        Console.WriteLine();
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine($"Error de comunicación: {ex.Message}");
        Console.WriteLine();
    }
}

decimal ReadStableWeight()
{
    using var serialPort = new SerialPort
    {
        PortName = portName,
        BaudRate = 9600,
        DataBits = 8,
        Parity = Parity.None,
        StopBits = StopBits.One,
        Handshake = Handshake.None,

        // Este timeout pertenece a cada intento individual
        // de lectura del puerto.
        ReadTimeout = 500
    };

    Console.WriteLine();
    Console.WriteLine($"Abriendo {portName}...");

    serialPort.Open();

    serialPort.DiscardInBuffer();

    Console.WriteLine(
        $"Esperando peso estable (máximo {stableWeightTimeoutSeconds} segundos)...");

    var stopwatch = Stopwatch.StartNew();

    while (stopwatch.Elapsed <
           TimeSpan.FromSeconds(stableWeightTimeoutSeconds))
    {
        try
        {
            string data = serialPort.ReadLine();

            if (TryParseStableWeight(
                    data,
                    out decimal weight))
            {
                Console.WriteLine(
                    $"Trama aceptada: {Escape(data)}");

                return weight;
            }
        }
        catch (TimeoutException)
        {
            // ReadLine no recibió una trama completa dentro
            // de ReadTimeout. Volvemos a intentarlo mientras
            // no se haya agotado el timeout global.
        }
    }

    throw new TimeoutException(
        $"No se pudo obtener un peso estable en {stableWeightTimeoutSeconds} segundos.");
}

bool TryParseStableWeight(
    string data,
    out decimal weight)
{
    weight = 0;

    string[] parts = data
        .Trim()
        .Split(',');

    if (parts.Length != 4)
        return false;

    string status = parts[0].Trim();
    string weightText = parts[2].Trim();
    string unit = parts[3].Trim();

    if (status != "ST")
        return false;

    if (!unit.Equals(
            "kg",
            StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    return decimal.TryParse(
        weightText,
        NumberStyles.Number,
        CultureInfo.InvariantCulture,
        out weight);
}

string Escape(string value)
{
    return value
        .Replace("\r", "\\r")
        .Replace("\n", "\\n");
}