using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Common;

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
            throw new NotFoundException(
                "No se encontró el ticket.");

        ticket.RegisterAmount(
            command.Discount,
            command.PricePerKg);

        await _repository.UpdateAsync(ticket);
    }
}