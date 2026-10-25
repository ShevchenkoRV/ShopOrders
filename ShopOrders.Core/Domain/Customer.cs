namespace ShopOrders.Core.Domain;

/// <summary>Покупець інтернет-магазину.</summary>
public sealed class Customer
{
    /// <summary>Внутрішній номер покупця.</summary>
    public int Id { get; init; }

    /// <summary>Прізвище та ім’я покупця.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Ознака постійного покупця.</summary>
    public bool IsRegular { get; init; }
}
