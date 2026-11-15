using System;
using ShopOrders.Core.Abstractions;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Pricing;

/// <summary>
/// Знижка за заданою часткою.
/// </summary>
public sealed class DiscountPricingPolicy : IPricingPolicy
{
    private readonly decimal _rate;

    /// <summary>
    /// Створює політику зі знижкою.
    /// </summary>
    /// <param name="rate">Частка знижки від 0 до 0.5.</param>
    public DiscountPricingPolicy(decimal rate)
    {
        if (rate < 0m || rate > 0.5m)
        {
            throw new ArgumentOutOfRangeException(nameof(rate));
        }

        _rate = rate;
    }

    /// <inheritdoc />
    public decimal PriceOf(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(line.Amount * (1m - _rate), 2, MidpointRounding.AwayFromZero);
    }
}
