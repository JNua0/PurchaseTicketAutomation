using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Printing.PrintTest;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Print;
using PurchaseTicket.Domain.Exceptions;

namespace PurchaseTicket.Desktop.Services;

// Ejecuta la impresion y traduce sus fallas a mensajes para el usuario.
// Una falla de impresion nunca deshace el registro del ticket
public sealed class TicketPrintService : ITicketPrintService
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketPrintService> _logger;

    // Constructor

    public TicketPrintService(
        IServiceScopeFactory scopeFactory,
        ILogger<TicketPrintService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // Metodos publicos

    public async Task<string?> PrintAsync(int ticketId)
    {
        try
        {
            // La comunicacion serial bloquea: se ejecuta fuera del hilo de la interfaz
            var result = await Task.Run(async () =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                return await scope.ServiceProvider
                    .GetRequiredService<PrintPurchaseTicket>()
                    .ExecuteAsync(new PrintPurchaseTicketCommand(ticketId));
            });

            // El caso de uso ya registro el detalle de la falla en el log
            return result.Printed
                ? null
                : DescribeError(result.ErrorCode);
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            return ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No fue posible solicitar la impresión del ticket {TicketId}.",
                ticketId);

            return "Ocurrió un error al solicitar la impresión.";
        }
    }

    public async Task<string?> PrintTestAsync()
    {
        try
        {
            var errorCode = await Task.Run(async () =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                return await scope.ServiceProvider
                    .GetRequiredService<PrintTestTicket>()
                    .ExecuteAsync();
            });

            return errorCode == PrinterErrorCode.None
                ? null
                : DescribeError(errorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No fue posible solicitar el ticket de prueba.");

            return "Ocurrió un error al solicitar la impresión.";
        }
    }

    // Metodos privados

    private static string DescribeError(PrinterErrorCode errorCode)
    {
        return errorCode switch
        {
            PrinterErrorCode.NotConfigured =>
                "La impresora no está configurada. Elige su puerto en Configuración.",

            PrinterErrorCode.PortNotFound =>
                "El puerto de la impresora no está disponible; verifica la configuración.",

            PrinterErrorCode.PortAccessDenied =>
                "El puerto de la impresora está en uso por otra aplicación.",

            PrinterErrorCode.CommunicationError =>
                "Falló la comunicación con la impresora; verifica la conexión.",

            _ => "No fue posible imprimir el ticket."
        };
    }
}