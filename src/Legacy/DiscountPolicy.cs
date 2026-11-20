namespace ShopOrders.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, ClientKind kind, int doneCount)
    {
        var rate = kind switch
        {
            ClientKind.Vip => PricingRules.VipRate,
            ClientKind.Staff => PricingRules.StaffRate,
            _ => doneCount > PricingRules.LoyalOrders ? PricingRules.LoyalRate : 0m
        };
        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }
}
