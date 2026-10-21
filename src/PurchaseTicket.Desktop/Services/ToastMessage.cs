using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Feedback;

namespace PurchaseTicket.Desktop.Services;

// Un aviso en pantalla. Los de exito e informacion se cierran solos;
// las advertencias y los errores esperan a que el operador los lea y cierre
public sealed class ToastMessage
{
    // Constructor

    public ToastMessage(
        string message,
        StatusKind kind,
        Action<ToastMessage> close)
    {
        Message = message;
        Kind = kind;

        CloseCommand = new RelayCommand(() => close(this));
    }

    // Propiedades

    public string Message { get; }

    public StatusKind Kind { get; }

    public bool ClosesAutomatically =>
        Kind is StatusKind.Success or StatusKind.Info;

    // Comandos

    public RelayCommand CloseCommand { get; }
}