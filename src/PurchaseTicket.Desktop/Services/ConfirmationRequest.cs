namespace PurchaseTicket.Desktop.Services;

// Lo que muestra la ventana de confirmacion. Title es la pregunta, como
// "¿Desactivar a Aceros Del Norte?", y Message explica que pasara
public sealed record ConfirmationRequest(
    string Title,
    string Message,
    string ConfirmText,
    ConfirmationKind Kind = ConfirmationKind.Danger,
    string? IconKey = null,
    string CancelText = "Cancelar");
