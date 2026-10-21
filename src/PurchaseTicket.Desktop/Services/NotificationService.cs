using System.Collections.ObjectModel;
using System.Windows.Threading;
using PurchaseTicket.Desktop.Components.Feedback;

namespace PurchaseTicket.Desktop.Services;

// Avisos apilados en la ventana principal.
// Se registra como singleton: todas las pantallas comparten la misma pila
public sealed class NotificationService : INotificationService
{
    // Campos

    // Mas avisos taparian la pantalla; al pasar el limite se quita el mas antiguo
    private const int MaxVisible = 4;

    private static readonly TimeSpan DisplayDuration = TimeSpan.FromSeconds(4);

    // Propiedades

    // Los nuevos se agregan al final y se muestran abajo
    public ObservableCollection<ToastMessage> Toasts { get; } = new();

    // Metodos publicos

    public void ShowSuccess(string message)
    {
        Show(message, StatusKind.Success);
    }

    public void ShowInfo(string message)
    {
        Show(message, StatusKind.Info);
    }

    public void ShowWarning(string message)
    {
        Show(message, StatusKind.Warning);
    }

    public void ShowError(string message)
    {
        Show(message, StatusKind.Error);
    }

    // Metodos privados

    private void Show(string message, StatusKind kind)
    {
        var toast = new ToastMessage(message, kind, Close);

        Toasts.Add(toast);

        while (Toasts.Count > MaxVisible)
            Toasts.RemoveAt(0);

        if (!toast.ClosesAutomatically)
            return;

        // Un temporizador por aviso: cada uno dura lo mismo desde que aparece.
        // DispatcherTimer ejecuta Tick en el hilo de la interfaz
        var timer = new DispatcherTimer
        {
            Interval = DisplayDuration
        };

        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Close(toast);
        };

        timer.Start();
    }

    // Si el operador ya lo cerro, Remove no hace nada
    private void Close(ToastMessage toast)
    {
        Toasts.Remove(toast);
    }
}