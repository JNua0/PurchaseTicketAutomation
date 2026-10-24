namespace PurchaseTicket.Domain.Enums;

// Datos de un ticket que pueden cambiar al corregirlo. Se guardan por nombre
// en el historial, asi que no se renombran
public enum TicketField
{
    Supplier,
    Material,
    LicensePlate,
    Transporter,
    GrossWeight,
    TareWeight,
    NetWeight,
    Discount,
    PricePerKg,
    Amount
}
