namespace ShopOrders.Core.Domain;

/// <summary>Замовлення покупця.</summary>
public sealed class Order
{
    private readonly List<OrderLine> _lines = new();

    /// <summary>Номер замовлення.</summary>
    public int Id { get; init; }

    /// <summary>Номер покупця, який оформив замовлення.</summary>
    public int CustomerId { get; init; }

    /// <summary>Поточний стан замовлення.</summary>
    public OrderStatus Status { get; set; } = OrderStatus.New;

    /// <summary>Дата й час створення замовлення.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Рядки замовлення, лише для читання.</summary>
    public IReadOnlyList<OrderLine> Lines => _lines;

    /// <summary>Додає рядок до замовлення.</summary>
    /// <param name="line">Рядок замовлення, що додається.</param>
    public void AddLine(OrderLine line) => _lines.Add(line);

    /// <summary>Обчислює суму замовлення.</summary>
    /// <returns>Сума вартостей усіх рядків замовлення.</returns>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (OrderLine line in _lines)
        {
            sum += line.Amount;
        }

        return sum;
    }
}
