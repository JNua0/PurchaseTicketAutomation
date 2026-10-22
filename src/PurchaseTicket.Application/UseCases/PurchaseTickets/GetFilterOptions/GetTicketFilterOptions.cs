using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.GetFilterOptions;

// Opciones de los filtros de proveedor y material en la pantalla Tickets.
// Salen de los tickets y no de los catalogos activos: un ticket viejo puede
// ser de un proveedor que hoy esta desactivado y aun asi debe poder filtrarse
public class GetTicketFilterOptions
{
    // Campos

    private readonly IPurchaseTicketQueryRepository _repository;

    // Constructor

    public GetTicketFilterOptions(
        IPurchaseTicketQueryRepository repository)
    {
        _repository = repository;
    }

    // Metodos publicos

    public async Task<TicketFilterOptions> ExecuteAsync()
    {
        var suppliers = await _repository.GetSuppliersInUseAsync();
        var materials = await _repository.GetMaterialsInUseAsync();

        return new TicketFilterOptions(suppliers, materials);
    }
}