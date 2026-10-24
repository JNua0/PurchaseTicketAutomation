using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PurchaseTicket.Desktop.Converters;

// Busca un recurso de la aplicacion por su clave, como "HomeIcon". Asi el
// ViewModel del menu nombra el icono sin depender de los recursos de WPF
public sealed class ResourceKeyConverter : IValueConverter
{
    // Metodos publicos

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string key
            ? System.Windows.Application.Current.TryFindResource(key)
            : null;
    }

    // Solo se usa para mostrar
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
