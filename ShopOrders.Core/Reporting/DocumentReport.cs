using System.Globalization;
using System.Text;
using ShopOrders.Core.Documents;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Reporting;

/// <summary>
/// Формування тексту звіту про документ.
/// </summary>
public static class DocumentReport
{
    private const string MoneyFormat = "F2";

    /// <summary>
    /// Будує текст звіту про документ. Результат не залежить від мови системи.
    /// </summary>
    /// <param name="request">Запит, за яким виконано розрахунок.</param>
    /// <param name="result">Результат розрахунку.</param>
    /// <returns>Текст звіту, готовий до виведення.</returns>
    public static string TextOf(TotalRequest request, TotalResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine(culture, $"Документ № {request.DocumentId}");
        text.AppendLine(culture, $"Клієнт: {request.Client.Name}");
        foreach (OrderLine line in request.Lines)
        {
            text.AppendLine(culture, $"{line.Sku} x {line.Quantity}");
        }

        text.AppendLine("Разом: " + result.Total.ToString(MoneyFormat, culture));
        text.AppendLine(culture, $"Стан: {result.Status}");
        return text.ToString();
    }
}
