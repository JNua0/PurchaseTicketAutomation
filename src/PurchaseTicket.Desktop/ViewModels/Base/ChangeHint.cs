namespace PurchaseTicket.Desktop.ViewModels.Base;

// Aviso debajo de un campo al editar: "Sin cambio" o "Antes 5,000".
// El estilo ChangeHintStyle pinta en gris "Sin cambio" y en ambar lo demas
public static class ChangeHint
{
    // Campos

    public const string Unchanged = "Sin cambio";

    // Metodos publicos

    public static string Describe(bool hasChanged, string originalText)
    {
        return hasChanged ? $"Antes {originalText}" : Unchanged;
    }
}
