using System.IO.Ports;
using System.Text;

const string portName = "COM3";

using var serialPort = new SerialPort
{
    PortName = portName,
    BaudRate = 9600,
    DataBits = 8,
    Parity = Parity.None,
    StopBits = StopBits.One,
    Handshake = Handshake.None,

    Encoding = Encoding.ASCII,

    ReadTimeout = 2000,
    WriteTimeout = 2000
};

try
{
    Console.WriteLine($"Abriendo {portName}...");

    serialPort.Open();

    Console.WriteLine($"{portName} abierto correctamente.");
    Console.WriteLine("Enviando prueba...");

    serialPort.Write("PRIMER TICKET DE COMPRA\r\n");
    serialPort.Write("\r\n");
    serialPort.Write("HOLA MUNDO\r\n");
    serialPort.Write("\r\n");
    serialPort.Write("PROVEEDOR: PRUEBA 1\r\n");
    serialPort.Write("MATERIAL: ACERO\r\n");

    Console.WriteLine("Datos enviados.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    if (serialPort.IsOpen)
    {
        serialPort.Close();
        Console.WriteLine($"{portName} cerrado.");
    }
}