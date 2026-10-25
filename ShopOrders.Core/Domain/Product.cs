namespace ShopOrders.Core.Domain;

/// <summary>Товар у каталозі магазину.</summary>
public sealed class Product
{
    /// <summary>Артикул товару.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Назва товару.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal Price { get; init; }

    /// <summary>Залишок на складі, одиниць.</summary>
    public int Stock { get; init; }
}
