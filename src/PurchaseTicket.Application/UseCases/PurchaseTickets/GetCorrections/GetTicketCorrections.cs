using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.Persistence.Models;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetCorrections;

// Historial de correcciones de un ticket para la ventana de detalle
public class GetTicketCorrections
{
    // Campos

    private readonly ITicketCorrectionQueryRepository _repository;

    // Constructor

    public GetTicketCorrections(
        ITicketCorrectionQueryRepository repository)
    {
        _repository = repository;
    }

    // Metodos publicos

    public async Task<IReadOnlyList<TicketCorrectionItem>> ExecuteAsync(int ticketId)
    {
        return await _repository.GetByTicketIdAsync(ticketId);
    }
}
