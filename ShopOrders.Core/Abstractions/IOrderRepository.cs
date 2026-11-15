using System.Collections.Generic;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Abstractions;

/// <summary>
/// Сховище замовлень: відповідає лише за зберігання.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Додає замовлення.
    /// </summary>
    /// <param name="order">Замовлення.</param>
    void Add(Order order);

    /// <summary>
    /// Шукає замовлення за ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор.</param>
    /// <returns>Замовлення або null.</returns>
    Order? GetById(int id);

    /// <summary>
    /// Повертає всі замовлення.
    /// </summary>
    /// <returns>Список замовлень лише для читання.</returns>
    IReadOnlyList<Order> GetAll();
}
