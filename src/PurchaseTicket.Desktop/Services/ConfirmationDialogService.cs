using System.Windows;
using PurchaseTicket.Desktop.Views.Shared;

namespace PurchaseTicket.Desktop.Services;

// Confirmaciones con el estilo de la app, en lugar del MessageBox de Windows
public class ConfirmationDialogService : IConfirmationDialogService
{
    // Metodos publicos

    public bool Confirm(ConfirmationRequest request)
    {
        var window = new ConfirmationWindow(request)
        {
            Owner = ActiveWindow()
        };

        return window.ShowDialog() == true;
    }

    // Metodos privados

    // Sobre la ventana que pidio la confirmacion, como el detalle del ticket
    private static Window ActiveWindow()
    {
        var application = System.Windows.Application.Current;

        return application.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive)
            ?? application.MainWindow;
    }
}
