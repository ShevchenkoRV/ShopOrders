namespace ShopOrders.Core.Documents;

/// <summary>
/// Дані клієнта, який оплачує документ.
/// </summary>
/// <param name="Name">Ім’я клієнта.</param>
/// <param name="Email">Електронна пошта клієнта.</param>
/// <param name="IsRegular">Ознака постійного клієнта.</param>
public sealed record ClientInfo(string Name, string Email, bool IsRegular);
