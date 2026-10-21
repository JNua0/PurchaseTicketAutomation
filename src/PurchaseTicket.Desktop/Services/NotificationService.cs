using System.Windows.Threading;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.Services;

// Aviso en la ventana principal que desaparece solo.
// Se registra como singleton: todas las pantallas comparten el mismo aviso.
public sealed class NotificationService : ObservableObject, INotificationService
{
    // Campos

    private static readonly TimeSpan DisplayDuration = TimeSpan.FromSeconds(3);

    private readonly DispatcherTimer _hideTimer;

    private string _message = string.Empty;
    private StatusKind _kind = StatusKind.Success;
    private bool _isVisible;

    // Constructor

    public NotificationService()
    {
        // DispatcherTimer ejecuta Tick en el hilo de la interfaz
        _hideTimer = new DispatcherTimer
        {
            Interval = DisplayDuration
        };

        _hideTimer.Tick += OnHideTimerTick;
    }

    // Propiedades

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public StatusKind Kind
    {
        get => _kind;
        private set => SetProperty(ref _kind, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        private set => SetProperty(ref _isVisible, value);
    }

    // Metodos publicos

    public void ShowSuccess(string message)
    {
        Show(message, StatusKind.Success);
    }

    public void ShowError(string message)
    {
        Show(message, StatusKind.Error);
    }

    // Metodos privados

    private void Show(string message, StatusKind kind)
    {
        Message = message;
        Kind = kind;
        IsVisible = true;

        // Si llega un aviso nuevo antes de ocultar el anterior, se reinicia el conteo
        _hideTimer.Stop();
        _hideTimer.Start();
    }

    private void OnHideTimerTick(object? sender, EventArgs e)
    {
        _hideTimer.Stop();

        IsVisible = false;
    }
}