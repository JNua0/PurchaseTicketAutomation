namespace PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterDepartureWeight;

public record RegisterDepartureWeightCommand(
    int TicketId,
    decimal TareWeight);