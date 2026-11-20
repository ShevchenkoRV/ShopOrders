using ShopOrders.Core;
using Xunit;

namespace ShopOrders.Tests;

public class OrderLineParserValidTests
{
    [Theory]
    [InlineData("SKU-1;2;10.50")]
    [InlineData(" SKU-2 ;1000;0")]
    public void TryParse_ValidInput_ReturnsLine(string raw)
    {
        var ok = OrderLineParser.TryParse(raw, out var line);

        Assert.True(ok);
        Assert.NotNull(line);
    }
}
