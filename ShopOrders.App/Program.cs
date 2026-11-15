using System;
using System.Globalization;
using ShopOrders.Core.Abstractions;
using ShopOrders.Core.Domain;
using ShopOrders.Core.Pricing;
using ShopOrders.Core.Services;
using ShopOrders.Core.Storage;

namespace ShopOrders.App;

/// <summary>
/// Композиційний корінь застосунку.
/// </summary>
internal static class Program
{
    private static void Main()
    {
        // Єдине місце, де програма створює конкретні реалізації
        var repository = new InMemoryOrderRepository();
        IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
        INotifier notifier = new ConsoleNotifier();
        var service = new OrderService(repository, pricing, notifier);

        var order = new Order(1, 100, new DateOnly(2026, 11, 15));
        order.AddLine(new OrderLine("SKU-1", 2, 250m));
        service.Place(order);

        decimal total = service.TotalOf(order);
        Console.WriteLine("Сума: " + total.ToString("0.00", new CultureInfo("uk-UA")));
    }
}
