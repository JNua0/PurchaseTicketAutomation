using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.RegisterAmount;

public class RegisterAmount
{
    private readonly IPurchaseTicketRepository _repository;

    public RegisterAmount(
        IPurchaseTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(
        RegisterAmountCommand command)
    {
        var ticket =
            await _repository.GetByIdAsync(
                command.TicketId);

        if (ticket is null)
            throw new InvalidOperationException(
                "Purchase ticket was not found.");

        ticket.RegisterAmount(
            command.Discount,
            command.PricePerKg);

        await _repository.UpdateAsync(ticket);
    }
}