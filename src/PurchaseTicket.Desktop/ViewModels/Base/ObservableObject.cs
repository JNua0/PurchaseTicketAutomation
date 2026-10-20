using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PurchaseTicket.Desktop.ViewModels.Base;

// Base de todos los ViewModels: notifica a la vista cuando cambia una propiedad
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    // Asigna el valor y notifica solo si cambio.
    // Devuelve true si hubo cambio, para notificar propiedades dependientes.
    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;

        OnPropertyChanged(propertyName);

        return true;
    }
}