using System.Text;
using ShopOrders.Core.Documents;
using ShopOrders.Core.Domain;
using ShopOrders.Core.Reporting;

Console.OutputEncoding = Encoding.UTF8;

TotalRequest request = BuildRequestFromInput();
TotalResult result = DocumentTotalCalculator.Calculate(request);
string report = DocumentReport.TextOf(request, result);
new DocumentReportPrinter(Console.Out).Print(report);

static TotalRequest BuildRequestFromInput()
{
    List<OrderLine> lines = new()
    {
        new OrderLine { Sku = "A1", Quantity = 12, UnitPrice = 100m },
        new OrderLine { Sku = "B2", Quantity = 1, UnitPrice = 250m },
    };
    ClientInfo client = new("Іваненко", "i@ex.com", true);
    DateOnly createdAt = new(2026, 3, 10);
    return new TotalRequest(1, client, lines, createdAt, null, DocumentStatus.New, 60m);
}
