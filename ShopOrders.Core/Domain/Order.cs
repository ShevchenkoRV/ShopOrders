using System;
using System.Collections.Generic;

namespace ShopOrders.Core.Domain;

/// <summary>
/// Замовлення: агрегат зі станом і контрольованими переходами.
/// </summary>
public sealed class Order
{
    private const int ReturnWindowDays = 14;

    private readonly List<OrderLine> _lines = new List<OrderLine>();

    /// <summary>
    /// Створює нове замовлення у стані New.
    /// </summary>
    /// <param name="id">Ідентифікатор, більший за нуль.</param>
    /// <param name="customerId">Ідентифікатор клієнта.</param>
    /// <param name="createdAt">Дата створення.</param>
    public Order(int id, int customerId, DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        Id = id;
        CustomerId = customerId;
        CreatedAt = createdAt;
        Status = OrderStatus.New;
    }

    /// <summary>Ідентифікатор замовлення.</summary>
    public int Id { get; }

    /// <summary>Ідентифікатор клієнта.</summary>
    public int CustomerId { get; }

    /// <summary>Дата створення.</summary>
    public DateOnly CreatedAt { get; }

    /// <summary>Поточний стан.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Дата відправлення, якщо замовлення відправлене.</summary>
    public DateOnly? ShippedAt { get; private set; }

    /// <summary>Позиції лише для читання.</summary>
    public IReadOnlyList<OrderLine> Lines => _lines;

    /// <summary>
    /// Додає позицію до нового замовлення.
    /// </summary>
    /// <param name="line">Позиція.</param>
    public void AddLine(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (Status != OrderStatus.New)
        {
            throw new InvalidOperationException("Позиції додають лише до нового документа");
        }

        _lines.Add(line);
    }

    /// <summary>
    /// Оплата: New у Paid, потрібна хоча б одна позиція.
    /// </summary>
    public void Pay()
    {
        Require(OrderStatus.New);
        if (_lines.Count == 0)
        {
            throw new InvalidOperationException("Документ порожній");
        }

        Status = OrderStatus.Paid;
    }

    /// <summary>
    /// Скасування: New у Cancelled.
    /// </summary>
    public void Cancel()
    {
        Require(OrderStatus.New);
        Status = OrderStatus.Cancelled;
    }

    /// <summary>
    /// Відправлення: Paid у Shipped.
    /// </summary>
    /// <param name="date">Дата відправлення.</param>
    public void Ship(DateOnly date)
    {
        Require(OrderStatus.Paid);
        ShippedAt = date;
        Status = OrderStatus.Shipped;
    }

    /// <summary>
    /// Повернення: Shipped у Returned не пізніше 14 днів після відправлення.
    /// </summary>
    /// <param name="date">Дата повернення.</param>
    public void AcceptReturn(DateOnly date)
    {
        Require(OrderStatus.Shipped);
        if (ShippedAt is null || date.DayNumber - ShippedAt.Value.DayNumber > ReturnWindowDays)
        {
            throw new InvalidOperationException("Строк повернення минув");
        }

        Status = OrderStatus.Returned;
    }

    private void Require(OrderStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException("Перехід заборонено");
        }
    }
}
