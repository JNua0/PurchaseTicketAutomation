using System.Globalization;

namespace PurchaseTicket.Infrastructure.Weighing;

internal static class DiniArgeoWeightParser
{
    public static bool TryParseStableWeight(
        string data,
        out decimal weight)
    {
        weight = 0;

        string[] parts = data
            .Trim()
            .Split(',');

        if (parts.Length != 4)
            return false;

        string status = parts[0].Trim();
        string weightText = parts[2].Trim();
        string unit = parts[3].Trim();

        if (status != "ST")
            return false;

        if (!unit.Equals(
                "kg",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return decimal.TryParse(
            weightText,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out weight);
    }
}