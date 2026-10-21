using PurchaseTicket.Application.Abstractions.Printing;

namespace PurchaseTicket.Desktop.Configuration;

// Datos de la empresa que se imprimen al inicio de cada ticket
public sealed class TicketHeaderSettings
{
    // Propiedades

    public string CompanyName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Neighborhood { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Metodos publicos

    // El encabezado tal como lo imprime la impresora: sin lineas vacias,
    // con "TEL." antes del telefono y la linea de cierre del ancho del papel
    public string ToPreview()
    {
        var lines = new List<string>();

        AddIfPresent(lines, CompanyName);
        AddIfPresent(lines, Address);
        AddIfPresent(lines, Neighborhood);

        if (!string.IsNullOrWhiteSpace(PhoneNumber))
            lines.Add($"TEL. {PhoneNumber.Trim()}");

        AddIfPresent(lines, Email);

        lines.Add(new string('=', TicketFormat.LineWidth));

        return string.Join(Environment.NewLine, lines);
    }

    // Metodos privados

    private static void AddIfPresent(List<string> lines, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            lines.Add(value.Trim());
    }
}