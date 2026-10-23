using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.ViewModels.Home;

// Fila de "En planta ahora": el ticket, cuanto lleva y si ya es mucho tiempo
public sealed record HomeInPlantRow(
    WeighingListItem Ticket,
    string Elapsed,
    bool IsLongStay)
{
    // Propiedades

    public string SupplierAndMaterial => $"{Ticket.SupplierName} · {Ticket.MaterialName}";
}
