namespace ShopOrders.Core.Domain;

/// <summary>Рядок замовлення: товар, кількість і ціна.</summary>
public sealed class OrderLine
{
    public string Sku { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal Amount => Quantity * UnitPrice;
}
