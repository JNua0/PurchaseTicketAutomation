using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Application.UseCases.Reports.Purchases;

// Reporte de compras de un periodo (CU-22). Cuentan los completados y los
// pendientes de importe; los cancelados y los vehiculos en planta no
public class GetPurchaseReport
{
    // Campos

    private readonly IReportQueryRepository _repository;

    // Constructor

    public GetPurchaseReport(IReportQueryRepository repository)
    {
        _repository = repository;
    }

    // Metodos publicos

    // El periodo es [fromUtc, toUtc), igual que el filtro de fechas de Tickets
    public async Task<PurchaseReport> ExecuteAsync(DateTime fromUtc, DateTime toUtc)
    {
        if (toUtc <= fromUtc)
            throw new ArgumentException("La fecha final debe ser posterior a la inicial.");

        var tickets = await _repository.GetTicketsAsync(fromUtc, toUtc);

        return PurchaseReport.Create(fromUtc, toUtc, tickets);
    }
}
