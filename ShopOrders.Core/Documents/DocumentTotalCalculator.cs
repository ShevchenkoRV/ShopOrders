using ShopOrders.Core.Domain;

namespace ShopOrders.Core.Documents;

/// <summary>
/// Розрахунок підсумку документа окремими короткими кроками.
/// </summary>
public static class DocumentTotalCalculator
{
    /// <summary>
    /// Рахує суму позицій з гуртовою знижкою.
    /// </summary>
    /// <param name="lines">Позиції документа.</param>
    /// <returns>Сума позицій після гуртових знижок.</returns>
    public static decimal SubtotalOf(IReadOnlyList<OrderLine> lines)
    {
        DocumentGuards.EnsureLinesValid(lines);
        decimal subtotal = 0m;
        foreach (OrderLine line in lines)
        {
            decimal sum = line.Quantity * line.UnitPrice;
            if (line.Quantity >= PricingRules.BulkQuantity)
            {
                sum -= sum * PricingRules.BulkRate;
            }

            subtotal += sum;
        }

        return subtotal;
    }

    /// <summary>
    /// Рахує знижку постійного клієнта.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <param name="isRegularClient">Ознака постійного клієнта.</param>
    /// <returns>Розмір знижки у гривнях.</returns>
    public static decimal ClientDiscountOf(decimal amount, bool isRegularClient)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegularClient ? amount * PricingRules.RegularClientRate : 0m;
    }

    /// <summary>
    /// Рахує знижку за обсягом замовлення.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <returns>Розмір знижки у гривнях.</returns>
    public static decimal VolumeDiscountOf(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (amount > PricingRules.LargeOrderFrom)
        {
            return amount * PricingRules.LargeOrderRate;
        }

        if (amount > PricingRules.MediumOrderFrom)
        {
            return amount * PricingRules.MediumOrderRate;
        }

        return 0m;
    }

    /// <summary>
    /// Рахує знижку за кодом купона.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <param name="couponCode">Код купона або null, якщо купона немає.</param>
    /// <returns>Розмір знижки у гривнях.</returns>
    public static decimal CouponDiscountOf(decimal amount, string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return couponCode switch
        {
            PricingRules.SaleCoupon => amount * PricingRules.CouponSaleRate,
            PricingRules.FixedCoupon when amount > PricingRules.CouponFixedFrom
                => PricingRules.CouponFixed,
            _ => 0m,
        };
    }

    /// <summary>
    /// Рахує знижку за день створення документа.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <param name="createdAt">Дата створення документа.</param>
    /// <returns>Розмір знижки у гривнях.</returns>
    public static decimal WeekendDiscountOf(decimal amount, DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return createdAt.DayOfWeek == DayOfWeek.Sunday ? amount * PricingRules.WeekendRate : 0m;
    }

    /// <summary>
    /// Рахує плату за доставку.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <param name="deliveryPrice">Вартість доставки.</param>
    /// <returns>Плата за доставку: нуль, якщо сума достатньо велика.</returns>
    public static decimal DeliveryFeeOf(decimal amount, decimal deliveryPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryPrice);
        return amount < PricingRules.FreeDeliveryFrom ? deliveryPrice : 0m;
    }

    /// <summary>
    /// Визначає наступний стан документа.
    /// </summary>
    /// <param name="current">Поточний стан.</param>
    /// <param name="total">Підсумкова сума документа.</param>
    /// <returns>Стан, у який документ переходить далі.</returns>
    public static DocumentStatus NextStatusOf(DocumentStatus current, decimal total)
    {
        return current switch
        {
            DocumentStatus.New when total > 0m => DocumentStatus.Paid,
            DocumentStatus.Paid => DocumentStatus.Shipped,
            _ => current,
        };
    }

    /// <summary>
    /// Диспетчер: викликає кроки розрахунку у фіксованому порядку.
    /// </summary>
    /// <param name="docId">Номер документа.</param>
    /// <param name="clientName">Ім’я клієнта.</param>
    /// <param name="clientEmail">Пошта клієнта.</param>
    /// <param name="isRegular">Ознака постійного клієнта.</param>
    /// <param name="lines">Позиції документа.</param>
    /// <param name="createdAt">Дата створення.</param>
    /// <param name="couponCode">Код купона або null.</param>
    /// <param name="status">Поточний стан документа.</param>
    /// <param name="printToConsole">Чи друкувати звіт у консоль.</param>
    /// <param name="deliveryPrice">Вартість доставки.</param>
    /// <param name="newStatus">Стан документа після розрахунку.</param>
    /// <returns>Підсумкова сума документа.</returns>
    public static decimal Process(
        int docId,
        string clientName,
        string clientEmail,
        bool isRegular,
        IReadOnlyList<OrderLine> lines,
        DateOnly createdAt,
        string? couponCode,
        DocumentStatus status,
        bool printToConsole,
        decimal deliveryPrice,
        out DocumentStatus newStatus)
    {
        DocumentGuards.EnsureHeaderValid(docId, clientName, clientEmail);
        decimal total = SubtotalOf(lines);
        total -= ClientDiscountOf(total, isRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, couponCode);
        total -= WeekendDiscountOf(total, createdAt);
        total += DeliveryFeeOf(total, deliveryPrice);
        total = status == DocumentStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), PricingRules.MoneyDecimals);
        newStatus = NextStatusOf(status, total);
        if (printToConsole)
        {
            Console.WriteLine("Документ № " + docId);
            Console.WriteLine("Клієнт: " + clientName);
            foreach (OrderLine line in lines)
            {
                Console.WriteLine(line.Sku + " x " + line.Quantity);
            }

            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }

        return total;
    }
}
