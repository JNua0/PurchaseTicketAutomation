namespace PurchaseTicket.Application.UseCases.Reports.Purchases;

// Un renglon de un desglose: un material o un proveedor con sus tickets del
// periodo. Amount suma solo los completados; PendingAmountCount son los que
// cuentan en kilos pero todavia no tienen importe
public sealed record PurchaseReportGroup(
    int Id,
    string Name,
    bool IsActive,
    int TicketCount,
    int PendingAmountCount,
    decimal NetWeight,
    decimal Amount);
