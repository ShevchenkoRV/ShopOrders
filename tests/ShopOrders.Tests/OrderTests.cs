using ShopOrders.Core.Domain;
using Xunit;

namespace ShopOrders.Tests;

public class OrderTests
{
    [Fact]
    public void Total_NoLines_ReturnsZero()
    {
        var order = new Order { Id = 1, CustomerId = 2 };

        Assert.Equal(0m, order.Total());
        Assert.Empty(order.Lines);
    }

    [Fact]
    public void Total_TwoLines_SumsAmounts()
    {
        var order = new Order();
        order.AddLine(new OrderLine { Sku = "A", Quantity = 2, UnitPrice = 10.5m });
        order.AddLine(new OrderLine { Sku = "B", Quantity = 1, UnitPrice = 4m });

        Assert.Equal(25m, order.Total());
        Assert.Equal(2, order.Lines.Count);
    }

    [Fact]
    public void Status_NewOrder_IsNewAndCanChange()
    {
        var order = new Order { CreatedAt = DateTimeOffset.UnixEpoch };

        Assert.Equal(OrderStatus.New, order.Status);
        order.Status = OrderStatus.Paid;
        Assert.Equal(OrderStatus.Paid, order.Status);
    }
}
