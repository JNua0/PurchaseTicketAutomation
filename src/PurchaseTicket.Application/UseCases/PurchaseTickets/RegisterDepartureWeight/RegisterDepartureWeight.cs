using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterDepartureWeight;

public class RegisterDepartureWeight
{
    private readonly IPurchaseTicketRepository _repository;

    public RegisterDepartureWeight(
        IPurchaseTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(
        RegisterDepartureWeightCommand command)
    {
        var ticket =
            await _repository.GetByIdAsync(
                command.TicketId);

        if (ticket is null)
            throw new InvalidOperationException(
                "Purchase ticket was not found.");

        ticket.RegisterTare(
            command.TareWeight);

        await _repository.UpdateAsync(ticket);
    }
}