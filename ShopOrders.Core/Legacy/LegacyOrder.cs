using System;
using System.Collections.Generic;

namespace ShopOrders.Core.Legacy;

public class LegacyLine
{
    public string Sku { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

public class LegacyOrder
{
    public int Id { get; set; }

    public int Status { get; set; }

    public decimal Total { get; set; }

    public string Email { get; set; } = string.Empty;

    public List<LegacyLine> Lines { get; set; } = new List<LegacyLine>();

    public virtual void AddLine(LegacyLine line)
    {
        Lines.Add(line);
    }
}

public class ArchivedOrder : LegacyOrder
{
    public ArchivedOrder(int id)
    {
        Id = id;
    }

    public override void AddLine(LegacyLine line)
    {
        throw new NotSupportedException("Архів змінювати не можна");
    }
}
