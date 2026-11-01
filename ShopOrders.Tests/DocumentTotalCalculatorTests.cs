using ShopOrders.Core.Documents;
using ShopOrders.Core.Domain;
using ShopOrders.Core.Reporting;
using Xunit;

namespace ShopOrders.Tests;

public class DocumentTotalCalculatorTests
{
    [Fact]
    public void Calculate_RegularClient_Returns1292()
    {
        TotalRequest request = new(
            DocumentId: 1,
            Client: new ClientInfo("Іваненко", "i@ex.com", true),
            Lines: new List<OrderLine>
            {
                new() { Sku = "A1", Quantity = 12, UnitPrice = 100m },
                new() { Sku = "B2", Quantity = 1, UnitPrice = 250m },
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: DocumentStatus.New,
            DeliveryPrice: 60m);

        TotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(DocumentStatus.Paid, result.Status);
    }

    [Fact]
    public void Calculate_SmallOrder_AddsDelivery()
    {
        TotalRequest request = BuildSampleRequest() with
        {
            Client = new ClientInfo("Петренко", "p@ex.com", false),
            Lines = new List<OrderLine> { new() { Sku = "A1", Quantity = 2, UnitPrice = 200m } },
            CouponCode = "SALE10",
            Status = DocumentStatus.Paid,
        };

        TotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(420m, result.Total);
        Assert.Equal(DocumentStatus.Shipped, result.Status);
    }

    [Fact]
    public void Calculate_CancelledDocument_ReturnsZero()
    {
        TotalRequest request = BuildSampleRequest() with { Status = DocumentStatus.Cancelled };

        TotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(0m, result.Total);
        Assert.Equal(DocumentStatus.Cancelled, result.Status);
    }

    [Fact]
    public void Calculate_SundayFixedCoupon_AppliesAllDiscounts()
    {
        TotalRequest request = BuildSampleRequest() with
        {
            Client = new ClientInfo("Петренко", "p@ex.com", false),
            Lines = new List<OrderLine>
            {
                new() { Sku = "C1", Quantity = 1, UnitPrice = 3000m },
                new() { Sku = "D2", Quantity = 1, UnitPrice = 3500m },
            },
            CreatedAt = new DateOnly(2026, 3, 8),
            CouponCode = "MINUS200",
            Status = DocumentStatus.Paid,
        };

        TotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(5537m, result.Total);
        Assert.Equal(DocumentStatus.Shipped, result.Status);
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

    [Fact]
    public void Calculate_BadEmail_ThrowsArgument()
    {
        TotalRequest request = BuildSampleRequest() with
        {
            Client = new ClientInfo("Іваненко", "no-at-sign", true),
        };

        Assert.Throws<ArgumentException>(() => DocumentTotalCalculator.Calculate(request));
    }

    [Fact]
    public void TextOf_ContainsTotalLine()
    {
        TotalRequest request = BuildSampleRequest();
        TotalResult result = DocumentTotalCalculator.Calculate(request);

        string text = DocumentReport.TextOf(request, result);

        Assert.Contains("Разом: 1292.70", text);
    }

    [Fact]
    public void Print_WritesReportToOutput()
    {
        StringWriter writer = new();
        DocumentReportPrinter printer = new(writer);

        printer.Print("Разом: 10.00");

        Assert.Contains("10.00", writer.ToString());
    }

    private static TotalRequest BuildSampleRequest()
    {
        return new TotalRequest(
            DocumentId: 1,
            Client: new ClientInfo("Іваненко", "i@ex.com", true),
            Lines: new List<OrderLine>
            {
                new() { Sku = "A1", Quantity = 12, UnitPrice = 100m },
                new() { Sku = "B2", Quantity = 1, UnitPrice = 250m },
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: DocumentStatus.New,
            DeliveryPrice: 60m);
    }
}
