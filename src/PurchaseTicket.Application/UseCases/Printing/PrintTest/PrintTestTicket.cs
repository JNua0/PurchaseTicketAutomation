using System.IO;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Devices;
using PurchaseTicket.Application.Abstractions.Printing;

namespace PurchaseTicket.Application.UseCases.Printing.PrintTest;

// Imprime un ticket sin datos de compra para comprobar el puerto de la
// impresora desde Configuracion. No consulta la base de datos
public class PrintTestTicket
{
    // Campos

    private readonly ITicketPrinter _ticketPrinter;
    private readonly ILogger<PrintTestTicket> _logger;

    // Constructor

    public PrintTestTicket(
        ITicketPrinter ticketPrinter,
        ILogger<PrintTestTicket> logger)
    {
        _ticketPrinter = ticketPrinter;
        _logger = logger;
    }

    // Metodos publicos

    // PrinterErrorCode.None si se imprimio; si no, el motivo de la falla
    public async Task<PrinterErrorCode> ExecuteAsync()
    {
        try
        {
            await _ticketPrinter.PrintTestAsync();

            return PrinterErrorCode.None;
        }
        catch (DeviceNotConfiguredException ex)
        {
            // Falta configurar el puerto: no es una falla de la impresora
            _logger.LogWarning(ex, "Ticket de prueba sin puerto de impresora configurado.");

            return PrinterErrorCode.NotConfigured;
        }
        catch (Exception ex)
        {
            // FileNotFoundException hereda de IOException, por eso va primero
            PrinterErrorCode errorCode = ex switch
            {
                FileNotFoundException => PrinterErrorCode.PortNotFound,
                UnauthorizedAccessException => PrinterErrorCode.PortAccessDenied,
                IOException => PrinterErrorCode.CommunicationError,
                _ => PrinterErrorCode.Unknown
            };

            _logger.LogError(
                ex,
                "Falló el ticket de prueba de la impresora: {ErrorCode}.",
                errorCode);

            return errorCode;
        }
    }
}