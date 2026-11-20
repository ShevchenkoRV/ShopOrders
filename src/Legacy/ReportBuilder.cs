using System.Text;

namespace ShopOrders.Legacy;

public sealed class ReportBuilder
{
    public string Build(int docId, string clientName, List<OrderLine> items,
        string currency, decimal discount, decimal shipping, decimal total)
    {
        var sb = new StringBuilder();
        sb.Append("Документ #").Append(docId).Append('\n');
        sb.Append("Клієнт: ").Append(clientName).Append('\n');
        foreach (var it in items)
            sb.Append(Line(it, currency));
        sb.Append(Money("Знижка", discount, currency));
        sb.Append(Money("Доставка", shipping, currency));
        sb.Append(Money("Разом", total, currency));
        return sb.ToString();
    }

    private static string Line(OrderLine it, string cur) =>
        it.Code + " x" + it.Qty + " = " +
        (it.Qty * it.Price).ToString("0.00") + " " + cur + "\n";

    private static string Money(string label, decimal v, string cur) =>
        label + ": " + v.ToString("0.00") + " " + cur + "\n";
}
