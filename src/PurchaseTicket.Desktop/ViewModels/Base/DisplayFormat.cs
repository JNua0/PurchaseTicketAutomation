using System.Globalization;

namespace PurchaseTicket.Desktop.ViewModels.Base;

// Formato de pesos e importes para mostrar, igual en todas las pantallas
public static class DisplayFormat
{
    // Metodos publicos

    // Pesos de la bascula, siempre enteros: "25,000 kg"
    public static string WholeKg(decimal weight)
    {
        return $"{weight.ToString("N0", CultureInfo.CurrentCulture)} kg";
    }

    // Pesos calculados con el descuento, a dos decimales: "598.20 kg"
    public static string DecimalKg(decimal weight)
    {
        return $"{weight.ToString("N2", CultureInfo.CurrentCulture)} kg";
    }

    // Importes y precio por kg: "$64,602.00"
    public static string Money(decimal amount)
    {
        return $"${amount.ToString("N2", CultureInfo.CurrentCulture)}";
    }

    // Telefono guardado solo con digitos. Los de 10 se agrupan: "911 234 5678";
    // los de otra longitud, como uno con lada internacional, se muestran igual
    public static string PhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return "Sin registrar";

        return phoneNumber.Length == 10 && phoneNumber.All(char.IsAsciiDigit)
            ? $"{phoneNumber[..3]} {phoneNumber[3..6]} {phoneNumber[6..]}"
            : phoneNumber;
    }
}
