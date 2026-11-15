using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Services;

/// <summary>
/// Форматувальник CSV-звіту: нічого не пише на диск.
/// </summary>
public sealed class OrderCsvReport
{
    /// <summary>
    /// Формує текст CSV за списком замовлень.
    /// </summary>
    /// <param name="orders">Замовлення.</param>
    /// <returns>Рядок CSV.</returns>
    public static string BuildCsv(IReadOnlyList<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);
        var text = new StringBuilder("id;status");
        foreach (Order order in orders)
        {
            text.Append(Environment.NewLine);
            text.Append(order.Id.ToString(CultureInfo.InvariantCulture));
            text.Append(';');
            text.Append(order.Status.ToString());
        }

        return text.ToString();
    }
}
