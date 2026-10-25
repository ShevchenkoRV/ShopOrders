namespace ShopOrders.Domain;

/// <summary>
/// Замовлення покупця: склад рядків, стан і розрахунок суми до сплати.
/// </summary>
public class Order
{
    /// <summary>Стан замовлення: новий.</summary>
    public const int StatusNew = 0;

    /// <summary>Стан замовлення: оплачений.</summary>
    public const int StatusPaid = 1;

    /// <summary>Стан замовлення: відправлений.</summary>
    public const int StatusShipped = 2;

    /// <summary>Стан замовлення: скасований.</summary>
    public const int StatusCancelled = 3;

    /// <summary>Ставка податку на додану вартість.</summary>
    private const decimal VatRate = 0.2m;

    /// <summary>Поріг суми для знижки постійному покупцю.</summary>
    private const decimal RegularDiscountThreshold = 1000m;

    /// <summary>Частка знижки постійному покупцю.</summary>
    private const decimal RegularDiscountRate = 0.10m;

    /// <summary>Поріг суми для знижки на велике замовлення.</summary>
    private const decimal LargeOrderThreshold = 5000m;

    /// <summary>Частка знижки на велике замовлення.</summary>
    private const decimal LargeOrderDiscountRate = 0.15m;

    /// <summary>Кількість рядків, з якої діє гуртова знижка.</summary>
    private const int BulkLineCount = 10;

    /// <summary>Сума гуртової знижки, грн.</summary>
    private const decimal BulkDiscountAmount = 100m;

    /// <summary>Найбільша допустима кількість рядків у замовленні.</summary>
    private const int MaxLineCount = 100;

    /// <summary>Найкоротше допустиме прізвище покупця.</summary>
    private const int MinCustomerNameLength = 3;

    private readonly List<string[]> _lines = new List<string[]>();

    /// <summary>
    /// Створює нове замовлення у стані «новий».
    /// </summary>
    /// <param name="id">Номер замовлення.</param>
    /// <param name="customerName">Прізвище покупця.</param>
    public Order(string id, string customerName)
    {
        Id = id;
        CustomerName = customerName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>
    /// Номер замовлення.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Прізвище покупця.
    /// </summary>
    public string CustomerName { get; private set; }

    /// <summary>
    /// Поточний стан: одне зі значень від StatusNew до StatusCancelled
    /// (за замовчуванням StatusNew).
    /// </summary>
    public int Status { get; private set; }

    /// <summary>
    /// Дата й час створення замовлення.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Шукає замовлення за номером у списку.
    /// </summary>
    /// <param name="orders">Список замовлень, у якому виконується пошук.</param>
    /// <param name="orderId">Шуканий номер замовлення.</param>
    /// <returns>Перше знайдене замовлення або null, якщо такого немає.</returns>
    public static Order FindById(List<Order> orders, string orderId)
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].Id == orderId)
            {
                return orders[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Додає до замовлення рядок із товаром.
    /// </summary>
    /// <param name="sku">Артикул товару.</param>
    /// <param name="quantity">Кількість одиниць.</param>
    /// <param name="unitPrice">Ціна за одиницю, грн.</param>
    public void AddLine(string sku, int quantity, decimal unitPrice)
    {
        string[] line = new string[3];
        line[0] = sku;
        line[1] = quantity.ToString();
        line[2] = unitPrice.ToString();
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює підсумкову суму замовлення з урахуванням знижок і податку.
    /// </summary>
    /// <param name="isRegularCustomer">Ознака постійного покупця.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularCustomer)
    {
        decimal total = 0;
        int lineCount = 0;

        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1]);
            decimal unitPrice = decimal.Parse(_lines[i][2]);
            total = total + quantity * unitPrice;
            lineCount = lineCount + 1;
        }

        // Знижка постійному покупцю і знижка на велике замовлення не поєднуються
        if (isRegularCustomer && total > RegularDiscountThreshold)
        {
            total *= 1m - RegularDiscountRate;
        }
        else if (total > LargeOrderThreshold)
        {
            total *= 1m - LargeOrderDiscountRate;
        }

        if (lineCount > BulkLineCount)
        {
            total -= BulkDiscountAmount;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total += total * VatRate;
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан замовлення, якщо перехід дозволений
    /// правилами предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий стан замовлення.</param>
    /// <returns>
    /// true, якщо перехід виконано; false, якщо він заборонений.
    /// </returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == StatusNew && newStatus == StatusPaid)
        {
            Status = StatusPaid;
            return true;
        }

        if (Status == StatusPaid && newStatus == StatusShipped)
        {
            Status = StatusShipped;
            return true;
        }

        if (Status == StatusNew && newStatus == StatusCancelled)
        {
            Status = StatusCancelled;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Перевіряє, чи замовлення заповнене коректно.
    /// </summary>
    /// <returns>
    /// true, якщо замовлення пройшло всі перевірки; інакше false.
    /// </returns>
    public bool IsValid()
    {
        return Id != null
            && Id != ""
            && CustomerName != null
            && CustomerName.Length >= MinCustomerNameLength
            && _lines.Count > 0
            && _lines.Count < MaxLineCount
            && Status >= StatusNew
            && Status <= StatusCancelled;
    }

    /// <summary>
    /// Формує текстовий звіт про замовлення.
    /// </summary>
    /// <returns>Текст звіту: рядки замовлення та підсумкова сума.</returns>
    public string BuildReport()
    {
        string report = "";
        for (int i = 0; i < _lines.Count; i++)
        {
            string[] line = _lines[i];
            decimal amount = int.Parse(line[1]) * decimal.Parse(line[2]);
            report = report + "Товар: " + line[0] + "; кількість: " + line[1]
                + "; ціна: " + line[2] + "; сума: " + amount + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }
}
