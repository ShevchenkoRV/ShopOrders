using System;
using System.Collections.Generic;

namespace ShopOrders.Core.Domain;

/// <summary>
/// Архівний запис: лише показує дані замовлення, не є замовленням.
/// </summary>
public sealed class ArchivedOrder
{
    private readonly Order _order;

    /// <summary>
    /// Створює архівний запис.
    /// </summary>
    /// <param name="order">Замовлення, яке архівують.</param>
    /// <param name="archivedAt">Дата архівування.</param>
    public ArchivedOrder(Order order, DateOnly archivedAt)
    {
        ArgumentNullException.ThrowIfNull(order);
        _order = order;
        ArchivedAt = archivedAt;
    }

    /// <summary>Дата архівування.</summary>
    public DateOnly ArchivedAt { get; }

    /// <summary>Ідентифікатор замовлення.</summary>
    public int Id => _order.Id;

    /// <summary>Стан замовлення.</summary>
    public OrderStatus Status => _order.Status;

    /// <summary>Позиції замовлення лише для читання.</summary>
    public IReadOnlyList<OrderLine> Lines => _order.Lines;
}
