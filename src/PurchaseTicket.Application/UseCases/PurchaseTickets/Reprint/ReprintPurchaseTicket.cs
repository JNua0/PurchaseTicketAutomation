using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Reprint;

public class ReprintPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly ITicketPrinter _ticketPrinter;

    public ReprintPurchaseTicket(
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
            await _ticketPrinter.PrintFinalAsync(printData);

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