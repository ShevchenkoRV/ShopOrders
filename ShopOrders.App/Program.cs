using System.Globalization;
using ShopOrders.Core.Domain;

Order order = new()
{
    Id = 1,
    CustomerId = 10,
    CreatedAt = DateTimeOffset.Now,
};

order.AddLine(new OrderLine
{
    Sku = "SKU-001", Quantity = 2, UnitPrice = 149.50m,
});

order.AddLine(new OrderLine
{
    Sku = "SKU-002", Quantity = 1, UnitPrice = 89.90m,
});

string total = order.Total()
    .ToString("F2", CultureInfo.InvariantCulture);

Console.WriteLine($"Замовлення #{order.Id}");
Console.WriteLine($"Стан: {order.Status}");
Console.WriteLine($"Позицій: {order.Lines.Count}");
Console.WriteLine($"Сума: {total}");
