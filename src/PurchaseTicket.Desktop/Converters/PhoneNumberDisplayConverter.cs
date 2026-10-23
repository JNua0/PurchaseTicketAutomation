using System.Globalization;
using System.Windows.Data;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.Converters;

// Telefono para mostrar en una tabla; el formato vive en DisplayFormat
public sealed class PhoneNumberDisplayConverter : IValueConverter
{
    // Metodos publicos

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DisplayFormat.PhoneNumber(value as string);
    }

    // Solo se usa para mostrar
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
