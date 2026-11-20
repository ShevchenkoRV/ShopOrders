using ShopOrders.Core.Domain;
using Xunit;

namespace ShopOrders.Tests;

public class CatalogModelTests
{
    [Fact]
    public void OrderLine_Amount_IsQuantityTimesUnitPrice()
    {
        var line = new OrderLine { Sku = "SKU-1", Quantity = 3, UnitPrice = 2.5m };

        Assert.Equal(7.5m, line.Amount);
    }

    [Fact]
    public void Customer_Defaults_AreEmptyAndNotRegular()
    {
        var customer = new Customer();

        Assert.Equal(string.Empty, customer.Name);
        Assert.False(customer.IsRegular);
    }

    [Fact]
    public void Product_InitializedValues_AreStored()
    {
        var product = new Product { Sku = "SKU-1", Title = "Mug", Price = 99.9m, Stock = 5 };

        Assert.Equal("SKU-1", product.Sku);
        Assert.Equal("Mug", product.Title);
        Assert.Equal(99.9m, product.Price);
        Assert.Equal(5, product.Stock);
    }
}
