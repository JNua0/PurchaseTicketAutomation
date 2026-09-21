namespace PurchaseTicket.Application.Abstractions.TicketNumbers;

public interface ITicketNumberGenerator
{
    Task<string> GenerateAsync();
}