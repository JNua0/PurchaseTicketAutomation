using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.Common;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetDetail;

// Detalle de un ticket para consultarlo (CU-05) antes de corregirlo,
// reimprimirlo o cancelarlo. Devuelve el modelo de consulta tal cual:
// es de solo lectura y ya trae lo que la ventana necesita
public class GetPurchaseTicketDetail
{
    // Campos

    private readonly IPurchaseTicketQueryRepository _repository;

    // Constructor

    public GetPurchaseTicketDetail(
        IPurchaseTicketQueryRepository repository)
    {
        _repository = repository;
    }

    // Metodos publicos

    public async Task<PurchaseTicketDetail> ExecuteAsync(int ticketId)
    {
        return await _repository.GetDetailAsync(ticketId)
            ?? throw new NotFoundException("No se encontró el ticket.");
    }
}