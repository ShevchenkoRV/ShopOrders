using System.Globalization;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core;

// Межа між «брудними» даними ззовні та сутностями.
public static class OrderLineParser
{
    public static bool TryParse(string? raw, out OrderLine? line)
    {
        line = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var p = raw.Split(';');
        if (p.Length != 3 || p[0].Trim().Length is 0 or > 20)
        {
            return false;
        }

        var ci = CultureInfo.InvariantCulture;
        if (!int.TryParse(p[1], NumberStyles.Integer, ci, out var qty)
            || qty is < 1 or > 1000)
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
