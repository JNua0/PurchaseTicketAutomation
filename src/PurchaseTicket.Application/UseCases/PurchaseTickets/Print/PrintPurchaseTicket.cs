using System.IO;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.Exceptions;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Print;

public class PrintPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly ITicketPrinter _ticketPrinter;
    private readonly ILogger<PrintPurchaseTicket> _logger;

    public PrintPurchaseTicket(
        IPurchaseTicketRepository repository,
        ISupplierRepository supplierRepository,
        IMaterialRepository materialRepository,
        ITicketPrinter ticketPrinter,
        ILogger<PrintPurchaseTicket> logger)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _materialRepository = materialRepository;
        _ticketPrinter = ticketPrinter;
        _logger = logger;
    }

    public async Task<PrintPurchaseTicketResult> ExecuteAsync(PrintPurchaseTicketCommand command)
    {
        var ticket = await _repository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new NotFoundException("No se encontró el ticket.");

        var supplier = await _supplierRepository.GetByIdAsync(ticket.SupplierId);

        if (supplier is null)
            throw new NotFoundException("No se encontró el proveedor.");

        var material = await _materialRepository.GetByIdAsync(ticket.MaterialId);

        if (material is null)
            throw new NotFoundException("No se encontró el material.");

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
                (ticket.Status == TicketStatus.AmountPending ||
                ticket.Status == TicketStatus.Completed))
        {
            // El pesaje unico usa el mismo formato antes y despues del importe
            printOperation =
                () => _ticketPrinter.PrintSingleAsync(
                    printData);
        }
        else
        {
            throw new BusinessRuleException(
                "El ticket no puede imprimirse en su estado actual.");
        }

        try
        {
            await printOperation();

            return new PrintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: true);
        }
        catch (FileNotFoundException ex)
        {
            // El puerto configurado no existe
            _logger.LogError(
                ex,
                "No se encontró el puerto de la impresora al imprimir el ticket {TicketNumber}.",
                ticket.TicketNumber);

            return new PrintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false,
                ErrorCode: PrinterErrorCode.PortNotFound);
        }
        catch (UnauthorizedAccessException ex)
        {
            // El puerto no permite el acceso
            _logger.LogError(
                ex,
                "Acceso denegado al puerto de la impresora al imprimir el ticket {TicketNumber}.",
                ticket.TicketNumber);

            return new PrintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false,
                ErrorCode: PrinterErrorCode.PortAccessDenied);
        }
        catch (IOException ex)
        {
            // Error durante la comunicacion serial
            _logger.LogError(
                ex,
                "Error de comunicación con la impresora al imprimir el ticket {TicketNumber}.",
                ticket.TicketNumber);

            return new PrintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false,
                ErrorCode: PrinterErrorCode.CommunicationError);
        }
        catch (Exception ex)
        {
            // Error de impresion no identificado
            _logger.LogError(
                ex,
                "Error no identificado al imprimir el ticket {TicketNumber}.",
                ticket.TicketNumber);

            return new PrintPurchaseTicketResult(
                ticket.TicketNumber,
                Printed: false,
                ErrorCode: PrinterErrorCode.Unknown);
        }
    }
}