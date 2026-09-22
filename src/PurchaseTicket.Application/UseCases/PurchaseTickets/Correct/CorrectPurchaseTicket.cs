using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;

public class CorrectPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ITicketPrinter _ticketPrinter;

    public CorrectPurchaseTicket(
        IPurchaseTicketRepository repository,
        ITicketPrinter ticketPrinter)
    {
        _repository = repository;
        _ticketPrinter = ticketPrinter;
    }

    public async Task<CorrectPurchaseTicketResult> ExecuteAsync(
    CorrectPurchaseTicketCommand command)
    {
        var ticket =
            await _repository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new InvalidOperationException(
                "Purchase ticket was not found.");

        ticket.Correct(
            command.SupplierCustomerId,
            command.MaterialId,
            command.LicensePlate,
            command.DriverName,
            command.GrossWeight);

        await _repository.UpdateAsync(ticket);

        try
        {
            await _ticketPrinter.PrintInitialAsync(ticket);

            return new CorrectPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: true);
        }
        catch
        {
            return new CorrectPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false);
        }
    }
}