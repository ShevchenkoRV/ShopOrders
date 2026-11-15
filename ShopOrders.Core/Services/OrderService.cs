using System;
using ShopOrders.Core.Abstractions;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Services;

/// <summary>
/// Прикладний сервіс замовлень: сценарій і розрахунок.
/// </summary>
public sealed class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;

    /// <summary>
    /// Створює сервіс із залежностями.
    /// </summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <param name="notifier">Сповіщувач.</param>
    public OrderService(IOrderRepository repository, IPricingPolicy pricing, INotifier notifier)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    /// <summary>
    /// Розміщує замовлення та повідомляє про це.
    /// </summary>
    /// <param name="order">Замовлення.</param>
    public void Place(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _repository.Add(order);
        _notifier.Notify("customer-" + order.CustomerId, "Замовлення " + order.Id + " розміщено");
    }

    /// <summary>
    /// Рахує суму замовлення за політикою.
    /// </summary>
    /// <param name="order">Замовлення.</param>
    /// <returns>Сума до сплати.</returns>
    public decimal TotalOf(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        decimal total = 0m;
        foreach (OrderLine line in order.Lines)
        {
            total += _pricing.PriceOf(line);
        }

        return total;
    }
}
