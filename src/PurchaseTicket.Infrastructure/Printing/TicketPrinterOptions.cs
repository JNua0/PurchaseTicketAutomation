namespace PurchaseTicket.Infrastructure.Printing;

public sealed class TicketPrinterOptions
{
    public string CompanyName { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string Neighborhood { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}