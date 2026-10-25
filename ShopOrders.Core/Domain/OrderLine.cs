namespace ShopOrders.Core.Domain;

/// <summary>Рядок замовлення: товар, кількість і ціна.</summary>
public sealed class OrderLine
{
    /// <summary>Артикул товару.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Кількість одиниць.</summary>
    public int Quantity { get; init; }

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>Вартість рядка: кількість, помножена на ціну.</summary>
    public decimal Amount => Quantity * UnitPrice;
}
