using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Cancel;

public class CancelPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;

    public CancelPurchaseTicket(
        IPurchaseTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(
        CancelPurchaseTicketCommand command)
    {
        var ticket =
            await _repository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new InvalidOperationException(
                "Purchase ticket was not found.");

        ticket.Cancel();

        await _repository.UpdateAsync(ticket);
    }
}