using System.Windows;

namespace PurchaseTicket.Desktop.Components.Layout;

// Las ventanas que cargan sus datos ya abiertas crecen despues de que WPF
// las centra con CenterOwner, y al crecer hacia abajo se salen de la pantalla
public static class DialogPlacement
{
    // Metodos publicos

    // Se conecta a SizeChanged: vuelve a centrar en vertical cada vez que cambia el alto
    public static void KeepCenteredOnOwner(Window window)
    {
        window.SizeChanged += (_, e) =>
        {
            if (e.HeightChanged)
                CenterVertically(window);
        };
    }

    // Metodos privados

    private static void CenterVertically(Window window)
    {
        Rect workArea = SystemParameters.WorkArea;
        Window? owner = window.Owner;

        // Una ventana principal maximizada reporta su posicion fuera del
        // area visible; en ese caso se centra en el area de trabajo
        Rect ownerBounds = owner is null || owner.WindowState == WindowState.Maximized
            ? workArea
            : new Rect(owner.Left, owner.Top, owner.ActualWidth, owner.ActualHeight);

        double top = ownerBounds.Top + ((ownerBounds.Height - window.ActualHeight) / 2);

        // Nunca por encima ni por debajo del area de trabajo
        window.Top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - window.ActualHeight));
    }
}