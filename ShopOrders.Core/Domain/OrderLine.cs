using System;

namespace ShopOrders.Core.Domain;

/// <summary>
/// Незмінна позиція замовлення.
/// </summary>
public sealed record OrderLine
{
    /// <summary>
    /// Створює позицію та перевіряє її інваріанти.
    /// </summary>
    /// <param name="sku">Артикул товару.</param>
    /// <param name="quantity">Кількість, більша за нуль.</param>
    /// <param name="unitPrice">Ціна за одиницю, не менша за нуль.</param>
    public OrderLine(string sku, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("Порожній артикул", nameof(sku));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        ArgumentOutOfRangeException.ThrowIfLessThan(unitPrice, 0m);

        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>Артикул товару.</summary>
    public string Sku { get; }

    /// <summary>Кількість одиниць.</summary>
    public int Quantity { get; }

    /// <summary>Ціна за одиницю.</summary>
    public decimal UnitPrice { get; }

    /// <summary>Сума позиції (обчислюється, а не зберігається).</summary>
    public decimal Amount => UnitPrice * Quantity;
}
