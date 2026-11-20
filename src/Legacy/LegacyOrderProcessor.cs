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
        _tmpSum = 0m;
        for (int i = 0; i < lines.Count; i++)
        {
            _tmpSum += lines[i].Qty * lines[i].Price;
        }
        _tmpDiscount = 0m;
        if (clientKind == ClientKind.Vip)
        {
            _tmpDiscount = _tmpSum * PricingRules.VipRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        else if (clientKind == ClientKind.Staff)
        {
            _tmpDiscount = _tmpSum * PricingRules.StaffRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        else if (clientDone > PricingRules.LoyalOrders)
        {
            _tmpDiscount = _tmpSum * PricingRules.LoyalRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < PricingRules.FreeShipFrom) ship = PricingRules.ShipCost;
        decimal total = _tmpSum - _tmpDiscount + ship;
        string txt = "Документ #" + docId + "\n";
        txt += "Клієнт: " + clientName + "\n";
        for (int i = 0; i < lines.Count; i++)
        {
            txt += lines[i].Code + " x" + lines[i].Qty + " = " +
                (lines[i].Qty * lines[i].Price)
                .ToString("0.00") + " " + currency + "\n";
        }
        txt += "Знижка: " + _tmpDiscount.ToString("0.00") +
            " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") +
            " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") +
            " " + currency + "\n";
        if (sendMail)
        {
            _log.Add("mail -> " + clientMail);
        }
        return txt;
    }

    private static string? Validate(List<OrderLine>? items, DocState state, string? mail)
    {
        if (items is null) return "ERR: null";
        if (items.Count == 0) return "ERR: empty";
        if (state is not (DocState.New or DocState.Paid)) return "ERR: state";
        if (mail is null || !mail.Contains("@")) return "ERR: mail";
        return null;
    }

    public decimal Preview(ClientKind clientKind, int clientDone,
        List<OrderLine> items)
    {
        decimal s = 0m;
        foreach (var it in items)
        {
            s += it.Qty * it.Price;
        }
        decimal d = 0m;
        if (clientKind == ClientKind.Vip)
        {
            d = s * PricingRules.VipRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (clientKind == ClientKind.Staff)
        {
            d = s * PricingRules.StaffRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (clientDone > PricingRules.LoyalOrders)
        {
            d = s * PricingRules.LoyalRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        decimal sh = 0m;
        if (s - d < PricingRules.FreeShipFrom) sh = PricingRules.ShipCost;
        return s - d + sh;
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
