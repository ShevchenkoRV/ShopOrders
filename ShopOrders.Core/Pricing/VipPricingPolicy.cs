using System;
using ShopOrders.Core.Abstractions;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Pricing;

/// <summary>
/// Знижка 10 % для VIP-клієнтів.
/// </summary>
public sealed class VipPricingPolicy : IPricingPolicy
{
    /// <inheritdoc />
    public decimal PriceOf(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(line.Amount * 0.90m, 2, MidpointRounding.AwayFromZero);
    }
}
