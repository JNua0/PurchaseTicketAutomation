using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Actions;
using PurchaseTicket.Desktop.Services;

namespace PurchaseTicket.Desktop.Views.Shared;

// Ventana de confirmacion; la abre ConfirmationDialogService
public partial class ConfirmationWindow : Window
{
    // Constructor

    public ConfirmationWindow(ConfirmationRequest request)
    {
        Request = request;

        bool isDanger = request.Kind == ConfirmationKind.Danger;

        ConfirmVariant = isDanger ? ActionButtonVariant.Danger : ActionButtonVariant.Primary;
        IconBrush = FindBrush(isDanger ? "ErrorBrush" : "SuccessBrush");
        IconBackground = FindBrush(isDanger ? "ErrorSoftBrush" : "SuccessSoftBrush");
        IconData = (Geometry)FindResource(request.IconKey ?? (isDanger ? "WarningIcon" : "ActivateIcon"));

        ConfirmCommand = new RelayCommand(() => DialogResult = true);
        CancelCommand = new RelayCommand(() => DialogResult = false);

        InitializeComponent();

        DataContext = this;
    }

    // Propiedades

    public ConfirmationRequest Request { get; }

    public ActionButtonVariant ConfirmVariant { get; }

    // No se llama Icon: Window ya tiene esa propiedad, la del icono de la barra
    // de titulo, y el enlace leeria esa en lugar de esta
    public Geometry IconData { get; }

    public Brush IconBrush { get; }

    public Brush IconBackground { get; }

    public ICommand ConfirmCommand { get; }

    public ICommand CancelCommand { get; }

    // Metodos privados

    private Brush FindBrush(string key)
    {
        return (Brush)FindResource(key);
    }
}
