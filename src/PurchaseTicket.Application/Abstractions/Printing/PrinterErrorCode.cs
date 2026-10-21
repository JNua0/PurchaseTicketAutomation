namespace PurchaseTicket.Application.Abstractions.Printing;

public enum PrinterErrorCode
{
    None,
    NotConfigured,
    PortNotFound,
    PortAccessDenied,
    CommunicationError,
    Unknown
}