using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Create;

public class CreatePurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ITicketNumberGenerator _ticketNumberGenerator;
    private readonly ITicketPrinter _ticketPrinter;

    public CreatePurchaseTicket(
        IPurchaseTicketRepository repository,
        ITicketNumberGenerator ticketNumberGenerator,
        ITicketPrinter ticketPrinter)
    {
        _repository = repository;
        _ticketNumberGenerator = ticketNumberGenerator;
        _ticketPrinter = ticketPrinter;
    }

    public async Task<CreatePurchaseTicketResult> ExecuteAsync(
    CreatePurchaseTicketCommand command)
    {
        string ticketNumber =
            await _ticketNumberGenerator.GenerateAsync();

        var ticket = new Ticket(
            ticketNumber,
            command.SupplierId,
            command.MaterialId,
            command.LicensePlate,
            command.DriverName,
            command.GrossWeight);

        await _repository.AddAsync(ticket);

        try
        {
            await _ticketPrinter.PrintInitialAsync(ticket);

            return new CreatePurchaseTicketResult(
                ticket.TicketNumber,
                Printed: true);
        }
        catch
        {
            return new CreatePurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false);
        }
    }
}