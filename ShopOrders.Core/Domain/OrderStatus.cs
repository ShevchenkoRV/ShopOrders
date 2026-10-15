namespace ShopOrders.Core.Domain;

/// <summary>Стан замовлення в його життєвому циклі.</summary>
public enum OrderStatus
{
    New = 0,
    Paid = 1,
    Shipped = 2,
    Cancelled = 3,
    Returned = 4,
}
