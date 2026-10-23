using System.Globalization;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Desktop.ViewModels.Base;

// Captura del descuento y del precio por kg, compartida por Registrar importe
// y Corregir ticket. Las reglas son las del dominio (RN-38 y RN-39); aqui solo
// se convierte el texto y se arman los mensajes de cada campo
public static class AmountInput
{
    // Campos

    // Permite escribir "$3.50" o "1,250.00"; no permite negativos ni exponentes
    private const NumberStyles PriceStyles =
        NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowTrailingWhite
        | NumberStyles.AllowCurrencySymbol
        | NumberStyles.AllowThousands
        | NumberStyles.AllowDecimalPoint;

    // Metodos publicos

    // Solo digitos: sin signo, decimales ni separadores (RN-38)
    public static int? ParseDiscount(string text)
    {
        return int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.CurrentCulture, out int discount)
            ? discount
            : null;
    }

    public static decimal? ParsePricePerKg(string text)
    {
        return decimal.TryParse(text, PriceStyles, CultureInfo.CurrentCulture, out decimal price)
            ? price
            : null;
    }

    // El valor si se puede leer y cumple la regla; si no, null
    public static int? ValidDiscount(string text)
    {
        return ParseDiscount(text) is int discount
            && ExceptionMessages.FromRule(() => Ticket.ValidateDiscount(discount)) is null
                ? discount
                : null;
    }

    public static decimal? ValidPricePerKg(string text)
    {
        return ParsePricePerKg(text) is decimal price
            && ExceptionMessages.FromRule(() => Ticket.ValidatePricePerKg(price)) is null
                ? price
                : null;
    }

    // Mensaje para mostrar junto al campo, o null si el valor es valido
    public static string? DiscountError(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? "El descuento es obligatorio."
            : ParseDiscount(text) is not int discount ? "El descuento debe ser un número entero entre 0 y 99."
            : ExceptionMessages.FromRule(() => Ticket.ValidateDiscount(discount));
    }

    public static string? PricePerKgError(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? "El precio por kilogramo es obligatorio."
            : ParsePricePerKg(text) is not decimal price ? "Escribe el precio como un número, por ejemplo 3.50."
            : ExceptionMessages.FromRule(() => Ticket.ValidatePricePerKg(price));
    }

    // Precio guardado como texto editable del campo: "3.50"
    public static string FormatPriceInput(decimal? price)
    {
        return price?.ToString("0.00", CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
