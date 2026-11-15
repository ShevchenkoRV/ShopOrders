using System;
using ShopOrders.Core.Abstractions;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Pricing;

/// <summary>
/// Ціна без знижки.
/// </summary>
public sealed class RegularPricingPolicy : IPricingPolicy
{
    /// <inheritdoc />
    public decimal PriceOf(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line.Amount;
    }
}
