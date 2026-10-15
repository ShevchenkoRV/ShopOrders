namespace ShopOrders.Core.Domain;

/// <summary>Покупець інтернет-магазину.</summary>
public sealed class Customer
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool IsRegular { get; init; }
}
