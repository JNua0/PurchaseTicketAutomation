using System.Globalization;

namespace PurchaseTicket.Domain.Entities;

public class Supplier
{
    // Constructor

    public Supplier(string name, string? phoneNumber = null)
    {
        Name = NormalizeName(name);
        PhoneNumber = NormalizePhoneNumber(phoneNumber);
        IsActive = true;
    }

    // Propiedades

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public bool IsActive { get; private set; }

    // Metodos publicos

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del proveedor es obligatorio.",
                nameof(name));
        }

        name = name.Trim();

        if (name.Length > 50)
        {
            throw new ArgumentException(
                "El nombre del proveedor no puede exceder los 50 caracteres.",
                nameof(name));
        }

        var textInfo = CultureInfo
            .GetCultureInfo("es-MX")
            .TextInfo;

        return textInfo.ToTitleCase(name.ToLower(
            CultureInfo.GetCultureInfo("es-MX")));
    }

    public static string? NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        phoneNumber = phoneNumber.Trim();

        // El signo + solo se permite al inicio
        bool hasPlusSign = phoneNumber.StartsWith('+');

        string body = hasPlusSign
            ? phoneNumber[1..]
            : phoneNumber;

        // Quitar los separadores permitidos: espacios y guiones
        string digits = new(
            body
                .Where(c => c != ' ' && c != '-')
                .ToArray());

        bool isValid =
            digits.Length >= 10 &&
            digits.Length <= 15 &&
            digits.All(char.IsAsciiDigit);

        if (!isValid)
            throw new ArgumentException(
                "El teléfono debe tener entre 10 y 15 dígitos y solo puede iniciar con +.",
                nameof(phoneNumber));

        return hasPlusSign
            ? "+" + digits
            : digits;
    }

    public void UpdateName(string name)
    {
        Name = NormalizeName(name);
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = NormalizePhoneNumber(phoneNumber);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}