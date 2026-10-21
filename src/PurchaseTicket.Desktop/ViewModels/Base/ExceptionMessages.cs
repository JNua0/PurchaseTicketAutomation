namespace PurchaseTicket.Desktop.ViewModels.Base;

// Mensajes de excepciones listos para mostrar al usuario.
// Lo usan los formularios con y sin validacion por campo
public static class ExceptionMessages
{
    // .NET agrega " (Parameter 'x')" al mensaje; no es para el usuario
    public static string ForUser(ArgumentException ex)
    {
        if (ex.ParamName is null)
            return ex.Message;

        return ex.Message.Replace(
            $" (Parameter '{ex.ParamName}')",
            string.Empty);
    }
}