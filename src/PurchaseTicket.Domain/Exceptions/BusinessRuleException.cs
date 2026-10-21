namespace PurchaseTicket.Domain.Exceptions;

// Regla de negocio que el usuario puede provocar, como cancelar un ticket
// ya cancelado. Su mensaje esta pensado para mostrarse en pantalla; los
// estados internos imposibles siguen usando InvalidOperationException y
// se tratan como error tecnico
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}