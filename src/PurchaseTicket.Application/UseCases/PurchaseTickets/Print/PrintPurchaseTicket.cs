using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Print;

public class PrintPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly ITicketPrinter _ticketPrinter;

    public PrintPurchaseTicket(
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

    public async Task<PrintPurchaseTicketResult> ExecuteAsync(PrintPurchaseTicketCommand command)
    {
        var ticket = await _repository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new InvalidOperationException("Purchase ticket was not found.");

        var supplier = await _supplierRepository.GetByIdAsync(ticket.SupplierId);

        if (supplier is null)
            throw new InvalidOperationException("Supplier was not found.");

        var material = await _materialRepository.GetByIdAsync(ticket.MaterialId);

        if (material is null)
            throw new InvalidOperationException("Material was not found.");

        var printData = new TicketPrintData(
            ticket,
            supplier.Name,
            material.Name);

        Func<Task> printOperation;

        if (ticket.WeighingType == WeighingType.Conventional &&
            ticket.Status == TicketStatus.WeighingPending)
        {
            printOperation =
                () => _ticketPrinter.PrintConventionalInitialAsync(
                    printData);
        }
        else if (ticket.WeighingType == WeighingType.Conventional &&
                 ticket.Status == TicketStatus.AmountPending)
        {
            printOperation =
                () => _ticketPrinter.PrintConventionalFinalAsync(
                    printData);
        }
        else if (ticket.WeighingType == WeighingType.Conventional &&
                 ticket.Status == TicketStatus.Completed)
        {
            printOperation =
                () => _ticketPrinter.PrintConventionalCompletedAsync(
                    printData);
        }
        else if (ticket.WeighingType == WeighingType.Single &&
                 ticket.Status == TicketStatus.AmountPending)
        {
            printOperation =
                () => _ticketPrinter.PrintSingleAsync(
                    printData);
        }
        else
        {
            throw new InvalidOperationException(
                "Purchase ticket cannot be printed in its current state.");
        }

        try
        {
            await printOperation();

            return new PrintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: true);
        }
        catch
        {
            return new PrintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false);
        }
    }
}