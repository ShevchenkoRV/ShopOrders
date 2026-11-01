using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Documents;

/// <summary>
/// Вхідні дані для розрахунку підсумку документа.
/// </summary>
/// <param name="DocumentId">Номер документа.</param>
/// <param name="Client">Дані клієнта.</param>
/// <param name="Lines">Позиції документа.</param>
/// <param name="CreatedAt">Дата створення документа.</param>
/// <param name="CouponCode">Код купона або null, якщо купона немає.</param>
/// <param name="Status">Поточний стан документа.</param>
/// <param name="DeliveryPrice">Вартість доставки.</param>
public sealed record TotalRequest(
    int DocumentId,
    ClientInfo Client,
    IReadOnlyList<OrderLine> Lines,
    DateOnly CreatedAt,
    string? CouponCode,
    DocumentStatus Status,
    decimal DeliveryPrice);
