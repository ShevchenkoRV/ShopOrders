using System;
using System.Collections.Generic;

namespace ShopOrders.Legacy;

public class Buyer
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public ClientKind Kind = ClientKind.Regular;
    public int DoneCount;
    public DateTime SinceUtc;
}

public class OrderLine
{
    public string Code = "";
    public int Qty;
    public decimal Price;
    public string Group = "";
}

public class LegacyOrderProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();
    private readonly List<string> _log = new();

    public string Handle(int docId, int clientId,
        string clientName, string? clientMail,
        ClientKind clientKind, int clientDone,
        List<OrderLine>? items, DocState state,
        string currency, DateTime createdAt, bool sendMail)
    {
        var error = Validate(items, state, clientMail);
        if (error != null) return error;
        _log.Add("ok " + docId);
        var lines = items!;
        _tmpSum = Subtotal(lines);
        _tmpDiscount = _discounts.For(_tmpSum, clientKind, clientDone);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;
        if (sendMail)
        {
            _log.Add("mail -> " + clientMail);
        }
        return _report.Build(docId, clientName, lines, currency, _tmpDiscount, ship, total);
    }

    private static string? Validate(List<OrderLine>? items, DocState state, string? mail)
    {
        if (items is null) return "ERR: null";
        if (items.Count == 0) return "ERR: empty";
        if (state is not (DocState.New or DocState.Paid)) return "ERR: state";
        if (mail is null || !mail.Contains("@")) return "ERR: mail";
        return null;
    }

    private static decimal Subtotal(List<OrderLine> items)
    {
        decimal sum = 0m;
        foreach (var it in items) sum += it.Qty * it.Price;
        return sum;
    }

    public decimal Preview(ClientKind clientKind, int clientDone,
        List<OrderLine> items)
    {
        var subtotal = Subtotal(items);
        var payable = subtotal - _discounts.For(subtotal, clientKind, clientDone);
        return payable + _shipping.For(payable);
    }

    public string DescribeClient(Buyer c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == ClientKind.Vip) s += " [VIP]";
        if (c.DoneCount > PricingRules.LoyalOrders) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + c.Email.ToLower() + ">";
        int years = DateTime.Now.Year - c.SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
