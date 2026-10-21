namespace PurchaseTicket.Application.Common;

// El registro solicitado ya no existe, por ejemplo un ticket que se
// elimino o un identificador obsoleto en una lista abierta. Su mensaje
// se muestra al usuario tal cual
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}