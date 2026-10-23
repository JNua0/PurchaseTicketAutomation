using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Get;

public record PurchaseTicketDto(
    int Id,
    string TicketNumber,
    DateTime CheckInAt,
    DateTime? DepartureAt,
    int SupplierId,
    int MaterialId,
    string SupplierName,
    string MaterialName,
    string? LicensePlate,
    string Transporter,
    decimal? GrossWeight,
    decimal? NetWeight,
    decimal? FinalWeight,
    decimal? Amount,
    WeighingType WeighingType,
    TicketStatus Status)
{
    // Metodos publicos

    // La misma fila que devuelve el repositorio; la usan Tickets e Inicio
    public static PurchaseTicketDto From(PurchaseTicketListItem item)
    {
        return new PurchaseTicketDto(
            item.Id,
            item.TicketNumber,
            item.CheckInAt,
            item.DepartureAt,
            item.SupplierId,
            item.MaterialId,
            item.SupplierName,
            item.MaterialName,
            item.LicensePlate,
            item.Transporter,
            item.GrossWeight,
            item.NetWeight,
            item.FinalWeight,
            item.Amount,
            item.WeighingType,
            item.Status);
    }
}
