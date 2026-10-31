using ShopOrders.Core.Documents;
using ShopOrders.Core.Domain;
using ShopOrders.Core.Legacy;
using Xunit;

namespace ShopOrders.Tests;

public class LegacyProcessorTests
{
    [Fact]
    public void Process_RegularClient_Returns1292()
    {
        List<OrderLine> lines = new()
        {
            new OrderLine { Sku = "A1", Quantity = 12, UnitPrice = 100m },
            new OrderLine { Sku = "B2", Quantity = 1, UnitPrice = 250m },
        };
        DateOnly created = new(2026, 3, 10);

        decimal total = LegacyProcessor.Process(
            1, "Іваненко", "i@ex.com", true, lines,
            created, string.Empty, "New", false, 60m, out string next);

        Assert.Equal(1292.70m, total);
        Assert.Equal("Paid", next);
    }

    [Fact]
    public void Process_SmallOrder_AddsDelivery()
    {
        List<OrderLine> lines = new()
        {
            new OrderLine { Sku = "A1", Quantity = 2, UnitPrice = 200m },
        };
        DateOnly created = new(2026, 3, 10);

        decimal total = LegacyProcessor.Process(
            2, "Петренко", "p@ex.com", false, lines,
            created, "SALE10", "Paid", false, 60m, out string next);

        Assert.Equal(420m, total);
        Assert.Equal("Shipped", next);
    }

    [Fact]
    public void Calculate_NoLines_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(
            () => DocumentGuards.EnsureLinesValid(new List<OrderLine>()));
    }

    [Fact]
    public void Calculate_ZeroQuantity_ThrowsRange()
    {
        OrderLine line = new() { Sku = "A1", Quantity = 0, UnitPrice = 10m };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => DocumentGuards.EnsureLineValid(line));
    }
}
