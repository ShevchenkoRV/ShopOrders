using System;

namespace ShopOrders.Core.Domain;

/// <summary>
/// Незмінний запис довідника клієнтів.
/// </summary>
public sealed record Customer
{
    /// <summary>
    /// Створює клієнта та перевіряє його інваріанти.
    /// </summary>
    /// <param name="id">Ідентифікатор, більший за нуль.</param>
    /// <param name="name">Ім’я, не порожнє.</param>
    /// <param name="email">Пошта, що містить символ @.</param>
    public Customer(int id, string name, string email)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Порожнє ім’я", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ArgumentException("Некоректна пошта", nameof(email));
        }

        Id = id;
        Name = name;
        Email = email;
    }

    /// <summary>Ідентифікатор клієнта.</summary>
    public int Id { get; }

    /// <summary>Ім’я клієнта.</summary>
    public string Name { get; }

    /// <summary>Пошта клієнта.</summary>
    public string Email { get; }

    /// <summary>Ознака постійного клієнта.</summary>
    public bool IsRegular { get; init; }
}
