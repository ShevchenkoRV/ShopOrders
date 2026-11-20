namespace ShopOrders.Legacy;

public class OrderProcessor
{
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();
    private readonly List<string> _log = new();

    public string Handle(OrderRequest req)
    {
        var error = Validate(req);
        if (error != null) return error;
        _log.Add("ok " + req.DocId);
        var subtotal = Subtotal(req.Items!);
        var discount = _discounts.For(subtotal, req.Client);
        var shipping = _shipping.For(subtotal - discount);
        var total = subtotal - discount + shipping;
        if (req.SendMail)
        {
            _log.Add("mail -> " + req.Client.Email);
        }
        return _report.Build(req, discount, shipping, total);
    }

    private static string? Validate(OrderRequest req)
    {
        if (req.Items is null) return "ERR: null";
        if (req.Items.Count == 0) return "ERR: empty";
        if (req.State is not (DocState.New or DocState.Paid)) return "ERR: state";
        if (!req.Client.HasValidEmail) return "ERR: mail";
        return null;
    }

    private static decimal Subtotal(IReadOnlyList<OrderLine> items)
    {
        decimal sum = 0m;
        foreach (var it in items) sum += it.Qty * it.Price;
        return sum;
    }

    public decimal Preview(Buyer client, IReadOnlyList<OrderLine> items)
    {
        var subtotal = Subtotal(items);
        var payable = subtotal - _discounts.For(subtotal, client);
        return payable + _shipping.For(payable);
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
