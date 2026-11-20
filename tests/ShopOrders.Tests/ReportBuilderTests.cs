using ShopOrders.Core;
using Xunit;

namespace ShopOrders.Tests;

public class ReportBuilderTests
{
    [Fact]
    public void Build_WithRows_ContainsUpperTitleAndRows()
    {
        var builder = new ReportBuilder { Title = "abc" };

        var text = builder.Build(new List<string> { "row1", "row2" });

        Assert.Contains("ABC", text, StringComparison.Ordinal);
        Assert.Contains("row1", text, StringComparison.Ordinal);
        Assert.Contains("row2", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_NullRows_Throws()
    {
        var builder = new ReportBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }
}
