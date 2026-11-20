using ShopOrders.Core;
using Xunit;

namespace ShopOrders.Tests;

public class VersionInfoTests
{
    [Fact]
    public void Current_HasNoBuildMetadataSuffix()
    {
        Assert.False(string.IsNullOrWhiteSpace(VersionInfo.Current));
        Assert.DoesNotContain("+", VersionInfo.Current, StringComparison.Ordinal);
    }
}
