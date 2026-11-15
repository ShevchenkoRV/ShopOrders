using System;
using ShopOrders.Core.Abstractions;
using ShopOrders.Core.Domain;
using ShopOrders.Core.Pricing;
using ShopOrders.Core.Services;
using ShopOrders.Core.Storage;
using Xunit;

namespace ShopOrders.Tests;

public sealed class DomainInvariantTests
{
    [Fact]
    public void OrderLine_WithZeroQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderLine("SKU-1", 0, 10m));
    }

    [Fact]
    public void Order_AfterPay_RejectsNewLines()
    {
        Order order = NewOrder(1);
        order.AddLine(new OrderLine("SKU-1", 1, 10m));
        order.Pay();

        var extra = new OrderLine("SKU-2", 1, 5m);

        Assert.Throws<InvalidOperationException>(() => order.AddLine(extra));
    }

    [Fact]
    public void TotalOf_WithTenPercentDiscount_Returns180()
    {
        var service = new OrderService(
            new InMemoryOrderRepository(),
            new DiscountPricingPolicy(0.10m),
            new FakeNotifier());
        Order order = NewOrder(2);
        order.AddLine(new OrderLine("SKU-1", 2, 100m));

        Assert.Equal(180m, service.TotalOf(order));
    }

    private static Order NewOrder(int id)
    {
        return new Order(id, 100, new DateOnly(2026, 11, 15));
    }

    private sealed class FakeNotifier : INotifier
    {
        public void Notify(string recipient, string message)
        {
        }
    }
}
