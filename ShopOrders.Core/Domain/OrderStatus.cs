namespace ShopOrders.Core.Domain;

/// <summary>Стан замовлення в його життєвому циклі.</summary>
public enum OrderStatus
{
    /// <summary>Нове замовлення.</summary>
    New = 0,

    /// <summary>Оплачене замовлення.</summary>
    Paid = 1,

    /// <summary>Відправлене замовлення.</summary>
    Shipped = 2,

    /// <summary>Скасоване замовлення.</summary>
    Cancelled = 3,

    /// <summary>Повернене замовлення.</summary>
    Returned = 4,
}
