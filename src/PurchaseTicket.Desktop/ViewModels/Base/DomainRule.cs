namespace PurchaseTicket.Desktop.ViewModels.Base;

// Reglas del dominio usadas para comparar, no para validar: si el valor no
// cumple la regla no se lanza la excepcion, solo se devuelve false
public static class DomainRule
{
    // Metodos publicos

    // value es el valor normalizado, como quedaria guardado
    public static bool TryNormalize(Func<string?> rule, out string? value)
    {
        try
        {
            value = rule();

            return true;
        }
        catch (ArgumentException)
        {
            value = null;

            return false;
        }
    }
}
