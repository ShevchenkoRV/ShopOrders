namespace ShopOrders.Core.Documents;

/// <summary>
/// Стан документа (замовлення) у його життєвому циклі.
/// </summary>
public enum DocumentStatus
{
    /// <summary>Новий документ.</summary>
    New,

    /// <summary>Оплачений документ.</summary>
    Paid,

    /// <summary>Відправлений документ.</summary>
    Shipped,

    /// <summary>Скасований документ.</summary>
    Cancelled,
}
