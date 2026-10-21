using PurchaseTicket.Domain.Exceptions;

namespace PurchaseTicket.Application.Common;

// Ya existe un registro con el mismo nombre.
// Permite a la interfaz mostrar el error junto al campo del nombre.
public sealed class NameAlreadyExistsException : BusinessRuleException
{
    public NameAlreadyExistsException(string message)
        : base(message)
    {
    }
}