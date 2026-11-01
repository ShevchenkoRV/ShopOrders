namespace ShopOrders.Core.Documents;

/// <summary>
/// Іменовані константи правил ціноутворення.
/// </summary>
public static class PricingRules
{
    /// <summary>Кількість одиниць у позиції, з якої діє гуртова знижка.</summary>
    public const int BulkQuantity = 10;

    /// <summary>Частка гуртової знижки на позицію.</summary>
    public const decimal BulkRate = 0.05m;

    /// <summary>Частка знижки постійному клієнту.</summary>
    public const decimal RegularClientRate = 0.07m;

    /// <summary>Сума, понад яку діє знижка на велике замовлення.</summary>
    public const decimal LargeOrderFrom = 5000m;

    /// <summary>Частка знижки на велике замовлення.</summary>
    public const decimal LargeOrderRate = 0.10m;

    /// <summary>Сума, понад яку діє знижка на середнє замовлення.</summary>
    public const decimal MediumOrderFrom = 2000m;

    /// <summary>Частка знижки на середнє замовлення.</summary>
    public const decimal MediumOrderRate = 0.05m;

    /// <summary>Частка знижки за замовлення, створене в неділю.</summary>
    public const decimal WeekendRate = 0.02m;

    /// <summary>Частка знижки за купоном відсоткового типу.</summary>
    public const decimal CouponSaleRate = 0.10m;

    /// <summary>Фіксована знижка за купоном із сумою.</summary>
    public const decimal CouponFixed = 200m;

    /// <summary>Сума, понад яку діє купон із фіксованою знижкою.</summary>
    public const decimal CouponFixedFrom = 1000m;

    /// <summary>Сума, від якої доставка безкоштовна.</summary>
    public const decimal FreeDeliveryFrom = 1000m;

    /// <summary>Кількість знаків після коми в грошовій сумі.</summary>
    public const int MoneyDecimals = 2;

    /// <summary>Код купона з відсотковою знижкою.</summary>
    public const string SaleCoupon = "SALE10";

    /// <summary>Код купона з фіксованою знижкою.</summary>
    public const string FixedCoupon = "MINUS200";
}
