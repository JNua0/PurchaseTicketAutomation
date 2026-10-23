using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.Home.GetSummary;

// Resumen de la pantalla Inicio: vehiculos en planta, tickets pendientes de
// importe y los totales del dia. Las listas reutilizan la consulta paginada,
// del mas antiguo al mas reciente: arriba lo que lleva mas tiempo esperando
public class GetHomeSummary
{
    // Campos

    public const int ListSize = 5;

    private readonly IPurchaseTicketQueryRepository _repository;

    // Constructor

    public GetHomeSummary(IPurchaseTicketQueryRepository repository)
    {
        _repository = repository;
    }

    // Metodos publicos

    public async Task<HomeSummary> ExecuteAsync(GetHomeSummaryQuery query)
    {
        if (query.DayStartUtc >= query.DayEndUtc)
            throw new ArgumentException(
                "El inicio del día debe ser anterior a su fin.");

        // Uno tras otro: las consultas comparten el mismo DbContext
        var inPlant = await _repository.GetPagedAsync(
            page: 1,
            pageSize: ListSize,
            status: TicketStatus.WeighingPending,
            oldestFirst: true);

        var amountPending = await _repository.GetPagedAsync(
            page: 1,
            pageSize: ListSize,
            status: TicketStatus.AmountPending,
            oldestFirst: true);

        var today = await _repository.GetDayTotalsAsync(
            query.DayStartUtc,
            query.DayEndUtc);

        return new HomeSummary(
            inPlant.TotalCount,
            inPlant.Items.Select(PurchaseTicketDto.From).ToList(),
            amountPending.TotalCount,
            amountPending.Items.Select(PurchaseTicketDto.From).ToList(),
            today);
    }
}
