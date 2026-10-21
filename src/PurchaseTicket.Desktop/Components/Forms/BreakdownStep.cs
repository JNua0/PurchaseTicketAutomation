namespace PurchaseTicket.Desktop.Components.Forms;

// Paso de un calculo, como "− Tara · 1,950 kg". IsResult resalta el resultado
public sealed record BreakdownStep(
    string Label,
    string Value,
    bool IsResult = false);