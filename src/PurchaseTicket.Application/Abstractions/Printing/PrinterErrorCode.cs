namespace PurchaseTicket.Application.Abstractions.Printing;

public enum PrinterErrorCode
{
    None,
    PortNotFound,
    PortAccessDenied,
    CommunicationError,
    Unknown
}