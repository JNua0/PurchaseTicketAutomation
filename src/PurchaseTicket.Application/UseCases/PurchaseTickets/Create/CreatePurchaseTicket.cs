using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Create;

public class CreatePurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly ITicketNumberGenerator _ticketNumberGenerator;
    private readonly ITicketPrinter _ticketPrinter;

    public CreatePurchaseTicket(
        IPurchaseTicketRepository repository,
        ISupplierRepository supplierRepository,
        IMaterialRepository materialRepository,
        ITicketNumberGenerator ticketNumberGenerator,
        ITicketPrinter ticketPrinter)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _materialRepository = materialRepository;
        _ticketNumberGenerator = ticketNumberGenerator;
        _ticketPrinter = ticketPrinter;
    }

    public async Task<CreatePurchaseTicketResult> ExecuteAsync(
        CreatePurchaseTicketCommand command)
    {
        var supplier =
            await _supplierRepository.GetByIdAsync(
                command.SupplierId);

        if (supplier is null)
            throw new InvalidOperationException(
                "Supplier was not found.");

        var material =
            await _materialRepository.GetByIdAsync(
                command.MaterialId);

        if (material is null)
            throw new InvalidOperationException(
                "Material was not found.");

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

        var printData = new TicketPrintData(
            ticket,
            supplier.Name,
            material.Name);

        try
        {
            await _ticketPrinter.PrintInitialAsync(printData);

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