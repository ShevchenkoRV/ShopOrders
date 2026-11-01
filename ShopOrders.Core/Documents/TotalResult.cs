namespace ShopOrders.Core.Documents;

/// <summary>
/// Результат розрахунку підсумку документа.
/// </summary>
/// <param name="Total">Підсумкова сума до сплати.</param>
/// <param name="Status">Стан документа після розрахунку.</param>
public sealed record TotalResult(decimal Total, DocumentStatus Status);
