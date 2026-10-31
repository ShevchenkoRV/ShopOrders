namespace ShopOrders.Core.Documents;

/// <summary>
/// Стан документа (замовлення) у його життєвому циклі.
/// </summary>
public enum DocumentStatus
{
    New,
    Paid,
    Shipped,
    Cancelled,
}
