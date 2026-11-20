using System.Globalization;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core;

// Межа між «брудними» даними ззовні та сутностями.
public static class OrderLineParser
{
    private const int MaxSkuLength = 20;
    private const int MinQuantity = 1;
    private const int MaxQuantity = 1000;

    public static bool TryParse(string? raw, out OrderLine? line)
    {
        line = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var p = raw.Split(';');
        if (p.Length != 3 || p[0].Trim().Length is 0 or > MaxSkuLength)
        {
            return false;
        }

        var ci = CultureInfo.InvariantCulture;
        if (!int.TryParse(p[1], NumberStyles.Integer, ci, out var qty)
            || qty is < MinQuantity or > MaxQuantity)
        {
            return false;
        }

        if (!decimal.TryParse(p[2], NumberStyles.Number, ci, out var price)
            || price < 0m)
        {
            return false;
        }

        line = new OrderLine
        {
            Sku = p[0].Trim(),
            Quantity = qty,
            UnitPrice = price,
        };
        return true;
    }
}
