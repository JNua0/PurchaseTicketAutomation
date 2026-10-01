using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;

public class CorrectPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly ITicketPrinter _ticketPrinter;

    public CorrectPurchaseTicket(
        IPurchaseTicketRepository repository,
        ISupplierRepository supplierRepository,
        IMaterialRepository materialRepository,
        ITicketPrinter ticketPrinter)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _materialRepository = materialRepository;
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
            command.SupplierId,
            command.MaterialId,
            command.LicensePlate,
            command.DriverName,
            command.GrossWeight);

        await _repository.UpdateAsync(ticket);

        var supplier =
            await _supplierRepository.GetByIdAsync(
                ticket.SupplierId);

        if (supplier is null)
            throw new InvalidOperationException(
                "Supplier was not found.");

        var material =
            await _materialRepository.GetByIdAsync(
                ticket.MaterialId);

        if (material is null)
            throw new InvalidOperationException(
                "Material was not found.");

        var printData = new TicketPrintData(
            ticket,
            supplier.Name,
            material.Name);

        try
        {
            await _ticketPrinter.PrintInitialAsync(printData);

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