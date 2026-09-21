using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Complete;

public class CompletePurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ITicketPrinter _ticketPrinter;

    public CompletePurchaseTicket(
        IPurchaseTicketRepository repository,
        ITicketPrinter ticketPrinter)
    {
        _repository = repository;
        _ticketPrinter = ticketPrinter;
    }

    public async Task<CompletePurchaseTicketResult> ExecuteAsync(
        CompletePurchaseTicketCommand command)
    {
        var ticket =
            await _repository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new InvalidOperationException(
                "Purchase ticket was not found.");

        ticket.Complete(
            command.TareWeight,
            command.Discount,
            command.PricePerKg);

        await _repository.UpdateAsync(ticket);

        try
        {
            await _ticketPrinter.PrintFinalAsync(ticket);

            return new CompletePurchaseTicketResult(
                ticket.TicketNumber,
                Printed: true);
        }
        catch
        {
            return new CompletePurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false);
        }
    }
}