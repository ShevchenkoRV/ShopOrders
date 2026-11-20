namespace ShopOrders.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, Buyer c)
    {
        var rate = RateFor(c);
        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }

    private static decimal RateFor(Buyer c) => c.Kind switch
    {
        ClientKind.Vip => PricingRules.VipRate,
        ClientKind.Staff => PricingRules.StaffRate,
        _ => c.IsLoyal ? PricingRules.LoyalRate : 0m
    };
}
