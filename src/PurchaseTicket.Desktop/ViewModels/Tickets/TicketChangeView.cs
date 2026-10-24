using System.Globalization;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.ValueObjects;

namespace PurchaseTicket.Desktop.ViewModels.Tickets;

// Un dato que cambio: nombre, antes y despues con el formato de la app
public sealed class TicketChangeView
{
    // Constructor

    public TicketChangeView(TicketFieldChange change)
    {
        IsRecalculated = change.IsRecalculated;
        FieldText = IsRecalculated ? $"{FieldName(change.Field)} (recalculado)" : FieldName(change.Field);
        BeforeText = FormatValue(change.Field, change.Before);
        AfterText = FormatValue(change.Field, change.After);
    }

    // Propiedades

    public string FieldText { get; }

    public string BeforeText { get; }

    public string AfterText { get; }

    // Lo recalculado se distingue de lo que se capturo a mano
    public bool IsRecalculated { get; }

    // Metodos privados

    private static string FieldName(TicketField field) => field switch
    {
        TicketField.Supplier => "Proveedor",
        TicketField.Material => "Material",
        TicketField.LicensePlate => "Placas",
        TicketField.Transporter => "Chofer",
        TicketField.GrossWeight => "Peso bruto",
        TicketField.TareWeight => "Tara",
        TicketField.NetWeight => "Peso neto",
        TicketField.Discount => "Descuento",
        TicketField.PricePerKg => "Precio por kg",
        TicketField.Amount => "Importe",
        _ => field.ToString()
    };

    // Los valores se guardan como texto invariante; aqui se les da formato
    private static string FormatValue(TicketField field, string? value)
    {
        if (value is null)
            return field == TicketField.LicensePlate ? "Sin placas" : "—";

        bool isNumber = decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number);

        if (!isNumber)
            return value;

        return field switch
        {
            TicketField.GrossWeight or TicketField.TareWeight or TicketField.NetWeight => DisplayFormat.WholeKg(number),
            TicketField.Discount => $"{number.ToString("0", CultureInfo.CurrentCulture)} %",
            TicketField.PricePerKg or TicketField.Amount => DisplayFormat.Money(number),
            _ => value
        };
    }
}
