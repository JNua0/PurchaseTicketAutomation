using PurchaseTicket.Desktop.Components.Feedback;

namespace PurchaseTicket.Desktop.ViewModels.Home;

// Estado de un dispositivo en el encabezado de Inicio. Mismos colores que
// Configuracion: sin puerto en rojo, puerto guardado pero no detectado en ambar.
// Solo revisa que Windows vea el puerto; no envia nada al dispositivo
public sealed record HomeDeviceStatus(
    string Text,
    StatusKind Kind)
{
    // Metodos publicos

    public static HomeDeviceStatus Create(
        string deviceName,
        string? savedPort,
        IReadOnlySet<string> detectedPorts)
    {
        if (string.IsNullOrWhiteSpace(savedPort))
            return new($"{deviceName} · no configurado", StatusKind.Error);

        return detectedPorts.Contains(savedPort)
            ? new($"{deviceName} · {savedPort} conectado", StatusKind.Success)
            : new($"{deviceName} · {savedPort} no está conectado", StatusKind.Warning);
    }
}
