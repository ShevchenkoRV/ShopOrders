using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Documents;

/// <summary>
/// Перевірки передумов для документа та його позицій.
/// </summary>
internal static class DocumentGuards
{
    /// <summary>
    /// Перевіряє номер документа та дані клієнта.
    /// </summary>
    /// <param name="documentId">Номер документа, має бути додатним.</param>
    /// <param name="clientName">Ім’я клієнта, не порожнє.</param>
    /// <param name="clientEmail">Пошта клієнта, містить символ @.</param>
    public static void EnsureHeaderValid(int documentId, string clientName, string clientEmail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(documentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientEmail);
        if (!clientEmail.Contains('@'))
        {
            throw new ArgumentException("Пошта має містити символ @.", nameof(clientEmail));
        }
    }

    /// <summary>
    /// Перевіряє одну позицію документа.
    /// </summary>
    /// <param name="line">Позиція: артикул, кількість і ціна.</param>
    public static void EnsureLineValid(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentException.ThrowIfNullOrWhiteSpace(line.Sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(line.Quantity, nameof(line.Quantity));
        ArgumentOutOfRangeException.ThrowIfNegative(line.UnitPrice, nameof(line.UnitPrice));
    }

    /// <summary>
    /// Перевіряє список позицій: він не порожній і кожна позиція коректна.
    /// </summary>
    /// <param name="lines">Позиції документа.</param>
    public static void EnsureLinesValid(IReadOnlyList<OrderLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
        {
            throw new ArgumentException("Документ не містить жодної позиції.", nameof(lines));
        }

        foreach (OrderLine line in lines)
        {
            EnsureLineValid(line);
        }
    }
}
