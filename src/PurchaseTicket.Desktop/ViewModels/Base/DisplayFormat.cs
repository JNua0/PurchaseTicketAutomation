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
}
