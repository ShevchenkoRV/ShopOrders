using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Abstractions;

/// <summary>
/// Політика ціноутворення позиції.
/// </summary>
public interface IPricingPolicy
{
    /// <summary>
    /// Розраховує ціну позиції.
    /// </summary>
    /// <param name="line">Позиція.</param>
    /// <returns>Сума до сплати за позицію.</returns>
    decimal PriceOf(OrderLine line);
}
