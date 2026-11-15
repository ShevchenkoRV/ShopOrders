namespace ShopOrders.Core.Domain;

/// <summary>
/// Стан замовлення.
/// </summary>
public enum OrderStatus
{
    /// <summary>Нове замовлення.</summary>
    New,

    /// <summary>Оплачене замовлення.</summary>
    Paid,

    /// <summary>Відправлене замовлення.</summary>
    Shipped,

    /// <summary>Скасоване замовлення.</summary>
    Cancelled,

    /// <summary>Повернене замовлення.</summary>
    Returned
}
