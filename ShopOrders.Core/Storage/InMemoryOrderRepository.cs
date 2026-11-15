using System;
using System.Collections.Generic;
using ShopOrders.Core.Abstractions;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Storage;

/// <summary>
/// Сховище замовлень у пам’яті.
/// </summary>
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<int, Order> _orders = new Dictionary<int, Order>();

    /// <inheritdoc />
    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!_orders.TryAdd(order.Id, order))
        {
            throw new InvalidOperationException("Замовлення з таким Id уже є");
        }
    }

    /// <inheritdoc />
    public Order? GetById(int id)
    {
        _orders.TryGetValue(id, out Order? found);
        return found;
    }

    /// <inheritdoc />
    public IReadOnlyList<Order> GetAll()
    {
        return new List<Order>(_orders.Values);
    }
}
