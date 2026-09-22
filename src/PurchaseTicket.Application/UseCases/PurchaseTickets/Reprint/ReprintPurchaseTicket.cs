using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Reprint;

public class ReprintPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ITicketPrinter _ticketPrinter;

    public ReprintPurchaseTicket(
        IPurchaseTicketRepository repository,
        ITicketPrinter ticketPrinter)
    {
        _repository = repository;
        _ticketPrinter = ticketPrinter;
    }

    public async Task<ReprintPurchaseTicketResult> ExecuteAsync(
        ReprintPurchaseTicketCommand command)
    {
        var ticket =
            await _repository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new InvalidOperationException(
                "Purchase ticket was not found.");

        if (ticket.Status != TicketStatus.Completed)
            throw new InvalidOperationException(
                "Only completed purchase tickets can be reprinted.");

        try
        {
            await _ticketPrinter.PrintFinalAsync(ticket);

            return new ReprintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: true);
        }
        catch
        {
            return new ReprintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false);
        }
    }
}